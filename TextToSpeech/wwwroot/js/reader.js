document.addEventListener('DOMContentLoaded', () => {
    const synth = window.speechSynthesis;

    const textInput = document.getElementById("text-input");
    const textContainer = document.getElementById("text-container");
    const voiceSelect = document.getElementById("voice-select");
    const rateSelect = document.getElementById("rate-select");

    const btnPlay = document.getElementById("btn-play");
    const btnPause = document.getElementById("btn-pause");
    const btnStop = document.getElementById("btn-stop");
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const statusText = document.getElementById("Status");

    let rawSegments = [];
    let currentSentenceIndex = 0;
    let IsPaused = false;
    let IsPlaying = false;
    let IsStopping = false;
    let availableVoices = [];
    let voicesLoadPromise = null;

    btnPrev.disabled = true;
    btnNext.disabled = true;

    function getRussianVoicesFirst(voice) {
        const ruVoices = voice.filter(v => v.lang.ToLowerCase().startsWith('ru'))
        const otherVoices = voice.filter(v => !v.lang.ToLowerCase().startsWith('ru'))

        return [...ruVoices, ...otherVoices];
    }

    function renderVoices(voices) {
        const selectedVoicesName = voiceSelect.value;

        voiceSelect.innerHTML = ''

        if (voices.length === 0) {
            const option = document.createElement('option');

            option.textContent = 'Голоса не найдены. Проверьте голоса Windows'
            option.value = '';

            voiceSelect.appendChild(option);
            voiceSelect.disabled = true;

            return;
        }

        voiceSelect.disabled = false;

        getRussianVoicesFirst(voices).forEach(voice => {
            const option = document.createElement('option');

            option.textContent = `${voice.name} (${voice.lang})`;
            option.value = voice.name;

            voiceSelect.appendChild(option);
        });

        const prefferedVoice = voices.find(v => v.name == selectedVoicesName)
            ?? voices.find(v => v.lang.ToLowerCase().startsWith('ru'));
        if (prefferedVoice) {
            voiceSelect.value = prefferedVoice.name;
        }
    }

    function loadVoices() {
        availableVoices = synth.getVoices();
        renderVoices(availableVoices);

        return availableVoices;
    }

    function waitForVoices() {
        if (voicesLoadPromise) {
            return voicesLoadPromise;
        }

        voicesLoadPromise = new Promise(resolve => {
            const maxAttempts = 20;
            let attempts = 0;

            const tryLoad = () => {
                const voices = loadVoices();

                if (voices.length > 0 || attempts >= maxAttempts) {
                    resolve(voices);
                    return;
                }

                attempts++;
                window.setTimeout(tryLoad, 250);
            }
            tryLoad();
        });
        return voicesLoadPromise;
    }
    if (!synth) {
        voiceSelect.innerHTML = 'option value="">Синтез речи не поддерживается</option>'
        voiceSelect.disabled = true;

        statusText.textContent = 'Ваш браузер не поддерживает Web Speech API';

        btnPlay.disabled = true;
        btnPause.disabled = true;
        btnStop.disabled = true;
        btnPrev.disabled = true;
        btnNext.disabled = true;

        return;
    }

    if (typeof synth.addEventListener === "function") {
        synth.addEventListener('voiceschanged', loadVoices);
    } else {
        synth.onvoiceschanged = loadVoices;
    }

    waitForVoices();

    function prepareTextDisplay(text) {
        rawSegments = [];
        currentSentenceIndex = 0;
        textContainer.innerHTML = '';

        if (!text.trim()) {
            textContainer.textContent = 'Введите текст для чтения...';
            return;
        }

        const parts = text.split(/([.!?\n]+)/);
        for (let i = 0; i < parts.length; i++) {
            if (!parts[i]) {
                continue;
            }

            if (/[.!?\n]/.test(parts[i])) {
                if (rawSegments.length > 0) {
                    rawSegments[rawSegments.length - 1] += parts[i];
                } else {
                    rawSegments.push(parts[i]);
                }
            } else {
                rawSegments.push(parts[i]);
            }
        }

        rawSegments.forEach((segment, index) => {
            const span = document.createElement('span');

            span.id = `seg-${index}`;
            span.textContent = segment;

            textContainer.appendChild(span);
        });
    }

    function clearHighLight() {
        const oldHighLight = textContainer.querySelector('.highlight');

        if (oldHighLight) {
            oldHighLight.classList.remove('highlight');
        }
    }

    function updateNavigationButtons() {
        btnPrev.disabled = !IsPlaying || currentSentenceIndex <= 0;
        btnNext.disabled = !IsPlaying || currentSentenceIndex >= rawSegments.length - 1;
    }

    function updateHighLight(index) {
        clearHighLight();

        const currentSpan = document.getElementById(`seg-${index}`);

        if (currentSpan) {
            currentSpan.classList.add('highlight');

            currentSpan.scrollIntoView({
                behavior: "smooth",
                block: "nearest"
            });
        }
        updateNavigationButtons();
    }

    async function saveSpeechLog() {
        const data = {
            text: textInput.value,
            voiceName: voiceSelect.value,
            rate: parseFloat(rateSelect.value)
        }

        try {
            const response = await fetch('/api/speechlogs', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });
            return;
        } catch {
            return false;
        }
    }

    function finishReading(message) {
        IsPlaying = false;
        IsPaused = false;
        IsStopping = false;

        statusText.textContent = message;
        updateNavigationButtons();
    }

    function speakCurrentSentence() {
        if (!IsPlaying) {
            return;
        }

        if (currentSentenceIndex >= rawSegments.length) {
            finishReading('Чтение завершено');
            return;
        }

        const textSegment = rawSegments[currentSentenceIndex].trim();

        if (!textSegment) {
            currentSentenceIndex++;
            speakCurrentSentence();
            return;
        }
        updateHighLight(currentSentenceIndex);
        const utterance = new SpeechSynthesisUtterance(textSegment);
        utterance.voice = availableVoices.find(v => v.name === voiceSelect.value) ??
            synth.getVoices().find(v => v.name === voiceSelect.value) ?? null;

        utterance.rate = parseFloat(rateSelect.value);

        utterance.onend = () => {
            if (IsStopping || !IsPlaying || IsPaused) {
                return;
            }

            currentSentenceIndex++;
            speakCurrentSentence();
        }

        utterance.onerror = () => {
            if (IsStopping) {
                return;
            }

            finishReading('Ошибка при озвучке');
        };

        statusText.textContent = `Читаю часть ${currentSentenceIndex + 1} из ${rawSegments.length}`;
        synth.speak(utterance);
    }

    btnPlay.addEventListener('click', async () => {
        await waitForVoices();

        if (IsPaused) {
            synth.resume();
            IsPaused = false;
            statusText.textContent = 'Продолжение чтения';
            updateNavigationButtons();
            return;
        }

        IsStopping = true;
        synth.cancel();

        prepareTextDisplay(textInput.value);

        IsStopping = false;
        IsPaused = false;

        if (rawSegments.length === 0) {
            finishReading('Введите текст');
            return;
        }

        IsPlaying = true;
        updateNavigationButtons();

        const isLogSaved = await saveSpeechLog();
        if (!isLogSaved) {
            statusText.textContent = 'Читаю без сохранения лога';
        }

        speakCurrentSentence();
    });

    btnPause.addEventListener('click', () => {
        if (synth.speaking && !IsPaused) {
            synth.pause();

            IsPaused = true;
            statusText.textContent = 'Пауза';
            updateNavigationButtons();
        }
    });

    btnStop.addEventListener('click', () => {
        IsStopping = true;
        IsPlaying = false;
        IsPaused = false;

        synth.cancel();

        currentSentenceIndex = 0;
        clearHighLight();
        statusText.textContent = 'Остановлено';

        updateNavigationButtons();
        window.setTimeout(() => {
            IsStopping = false;
        }, 0);
    });

    btnNext.addEventListener('click', () => {
        if (!IsPlaying || currentSentenceIndex >= rawSegments.length - 1) {
            return;
        }

        IsStopping = true;
        synth.cancel();

        IsPaused = false;
        currentSentenceIndex++;
        window.setTimeout(() => {
            IsStopping = false;
            speakCurrentSentence();
        }, 0);
    });

    btnPrev.addEventListener('click', () => {
        if (!IsPlaying || currentSentenceIndex <= 0) {
            return;
        }

        IsStopping = true;
        synth.cancel();

        IsPaused = false;
        currentSentenceIndex--;
        window.setTimeout(() => {
            IsStopping = false;
            speakCurrentSentence();
        }, 0);
    });


});