
document.addEventListener('DOMContentLoaded', () => {

    // --- Constants and Globals ---
    const API_BASE_URL = 'http://127.0.0.1:8000';
    let ecgChart; // To hold the chart instance

    // --- DOM Element References ---
    const ecgCanvas = document.getElementById('ecgChart');
    const arrhythmiaTypeSpan = document.getElementById('arrhythmia-type');
    const confidenceSpan = document.getElementById('confidence');
    const analyzeButton = document.getElementById('analyze-button');
    const chatHistory = document.getElementById('chat-history');
    const chatInput = document.getElementById('chat-input');
    const chatSendButton = document.getElementById('chat-send-button');

    // --- Mock Data Generator ---
    function generateMockEcgSignal() {
        // Generates 187 data points to match the model's input size
        const signal = [];
        for (let i = 0; i < 187; i++) {
            signal.push(Math.sin(i * 0.1) * 0.5 + Math.cos(i * 0.25) * 0.2 + (Math.random() - 0.5) * 0.1);
        }
        return signal;
    }

    // --- Charting Function ---
    function renderEcgChart(signalData) {
        const ctx = ecgCanvas.getContext('2d');
        if (ecgChart) {
            ecgChart.destroy(); // Clear previous chart instance
        }
        ecgChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: Array.from({ length: signalData.length }, (_, i) => i),
                datasets: [{
                    label: 'ECG Signal',
                    data: signalData,
                    borderColor: 'rgba(231, 76, 60, 1)',
                    borderWidth: 2,
                    pointRadius: 0,
                    fill: false
                }]
            },
            options: {
                scales: {
                    x: {
                        title: { display: true, text: 'Time (samples)' }
                    },
                    y: {
                        title: { display: true, text: 'Amplitude' }
                    }
                },
                animation: false
            }
        });
    }

    // --- API Communication ---
    async function analyzeSignal(signalData) {
        try {
            const response = await fetch(`${API_BASE_URL}/predict`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ signal: signalData })
            });
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            const data = await response.json();

            // Update the UI with the results
            arrhythmiaTypeSpan.textContent = data.arrhythmia_type;
            confidenceSpan.textContent = `${(data.confidence * 100).toFixed(2)}%`;
        } catch (error) {
            console.error('Error analyzing signal:', error);
            arrhythmiaTypeSpan.textContent = 'Error';
            confidenceSpan.textContent = 'Could not get a diagnosis.';
        }
    }

    async function sendMessageToChatbot(message) {
        try {
            const response = await fetch(`${API_BASE_URL}/chat`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: message })
            });
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            const data = await response.json();

            // Add bot's reply to the chat history
            addMessageToChat('Bot', data.reply, 'bot-message');
        } catch (error) {
            console.error('Error with chatbot:', error);
            addMessageToChat('Bot', 'Sorry, I am having trouble connecting.', 'bot-message');
        }
    }

    // --- Chat UI Helper ---
    function addMessageToChat(sender, message, messageClass) {
        const messageElement = document.createElement('div');
        messageElement.classList.add(messageClass);
        messageElement.textContent = message;
        chatHistory.appendChild(messageElement);
        // Scroll to the bottom
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }

    // --- Event Listeners ---
    analyzeButton.addEventListener('click', () => {
        const newSignal = generateMockEcgSignal();
        renderEcgChart(newSignal);
        analyzeSignal(newSignal);
    });

    chatSendButton.addEventListener('click', () => {
        const message = chatInput.value.trim();
        if (message) {
            addMessageToChat('You', message, 'user-message');
            sendMessageToChatbot(message);
            chatInput.value = '';
        }
    });

    chatInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            chatSendButton.click();
        }
    });

    // --- Initial Load ---
    function initialize() {
        const initialSignal = generateMockEcgSignal();
        renderEcgChart(initialSignal);
        // Optionally, analyze the first signal on load
        // analyzeSignal(initialSignal);
    }

    initialize();
});
