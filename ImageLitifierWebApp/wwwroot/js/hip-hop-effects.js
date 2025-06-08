document.addEventListener('DOMContentLoaded', function() {
    // Original form functionality
    const uploadForm = document.getElementById('uploadForm');
    const uploadSection = document.getElementById('uploadSection');
    const processingSection = document.getElementById('processingSection');
    const resultSection = document.getElementById('resultSection');
    const errorSection = document.getElementById('errorSection');
    const uploadBtn = document.getElementById('uploadBtn');
    const uploadBtnText = document.getElementById('uploadBtnText');
    const uploadSpinner = document.getElementById('uploadSpinner');
    const statusMessage = document.getElementById('statusMessage');
    const resultContainer = document.getElementById('resultContainer');
    const errorMessage = document.getElementById('errorMessage');

    // Audio elements
    const backgroundMusic = document.getElementById('backgroundMusic');
    const buttonSound = document.getElementById('buttonSound');
    const successSound = document.getElementById('successSound');
    const musicToggle = document.getElementById('musicToggle');

    let pollingInterval;
    let currentRequestId;
    let musicPlaying = false;

    // Music control
    musicToggle.addEventListener('click', function() {
        playButtonSound();
        if (musicPlaying) {
            backgroundMusic.pause();
            musicToggle.classList.remove('playing');
            musicToggle.textContent = '🎵';
        } else {
            backgroundMusic.play().catch(e => console.log('Audio play failed:', e));
            musicToggle.classList.add('playing');
            musicToggle.textContent = '🎶';
        }
        musicPlaying = !musicPlaying;
    });

    // Auto-start music on first user interaction
    document.addEventListener('click', function() {
        if (!musicPlaying) {
            backgroundMusic.play().catch(e => console.log('Audio play failed:', e));
            musicToggle.classList.add('playing');
            musicToggle.textContent = '🎶';
            musicPlaying = true;
        }
    }, { once: true });

    // Sound effect functions
    function playButtonSound() {
        buttonSound.currentTime = 0;
        buttonSound.play().catch(e => console.log('Button sound failed:', e));
    }

    function playSuccessSound() {
        successSound.currentTime = 0;
        successSound.play().catch(e => console.log('Success sound failed:', e));
    }

    // Add sound effects to all buttons
    document.querySelectorAll('.retro-btn').forEach(btn => {
        btn.addEventListener('click', playButtonSound);
    });

    // Enhanced form submission with hip-hop flair
    uploadForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        playButtonSound();

        const fileInput = document.getElementById('imageFile');
        const file = fileInput.files[0];

        if (!file) {
            showError('YO, YOU GOTTA SELECT AN IMAGE FIRST! 📸');
            return;
        }

        // Clear any previous states
        hideAllSections();

        // Show upload loading state with style
        uploadBtn.disabled = true;
        uploadBtnText.textContent = '🚀 UPLOADING... 🚀';
        uploadSpinner.classList.remove('d-none');

        const formData = new FormData();
        formData.append('imageFile', file);

        try {
            const response = await fetch('/Images/upload', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (response.ok) {
                currentRequestId = result.requestId;
                showProcessing();
                startPolling(result.requestId);
            } else {
                showError(result.message || 'UPLOAD FAILED, TRY AGAIN! 😤');
            }
        } catch (error) {
            showError('NETWORK ERROR: ' + error.message + ' 🌐💥');
        } finally {
            // Reset upload button
            uploadBtn.disabled = false;
            uploadBtnText.textContent = '🚀 UPLOAD & PROCESS 🚀';
            uploadSpinner.classList.add('d-none');
        }
    });

    function startPolling(requestId) {
        const statusMessages = [
            'COOKING UP THAT FIRE... 🔥',
            'ADDING SOME SAUCE... 🌶️',
            'MIXING THE BEATS... 🎵',
            'BRINGING THE HEAT... 🔥',
            'ALMOST READY... ⏰'
        ];
        let messageIndex = 0;

        pollingInterval = setInterval(async () => {
            try {
                const response = await fetch(`/Images/status/${requestId}`);
                const status = await response.json();

                if (response.ok) {
                    if (status.status === 'Completed') {
                        clearInterval(pollingInterval);
                        showResult(status.resultUrl);
                        playSuccessSound();
                    } else {
                        // Cycle through hip-hop status messages
                        statusMessage.textContent = statusMessages[messageIndex % statusMessages.length];
                        messageIndex++;
                    }
                } else {
                    clearInterval(pollingInterval);
                    showError('FAILED TO CHECK STATUS 📡💥');
                }
            } catch (error) {
                clearInterval(pollingInterval);
                showError('ERROR CHECKING STATUS: ' + error.message + ' 🚨');
            }
        }, 2000);
    }

    function showProcessing() {
        hideAllSections();
        processingSection.style.display = 'block';
        // Add some visual flair
        processingSection.style.animation = 'slideInUp 0.5s ease-out';
    }

    function showResult(imageUrl) {
        hideAllSections();
        resultContainer.innerHTML = `
            <div class="result-image-container">
                <img src="${imageUrl}" class="img-fluid rounded shadow result-image" alt="Processed Flame GIF" style="max-height: 400px; border: 3px solid #00ff00; box-shadow: 0 0 20px rgba(0, 255, 0, 0.6);">
                <div class="mt-3">
                    <a href="${imageUrl}" class="retro-btn primary-btn download-btn" download>💾 DOWNLOAD YOUR FIRE 💾</a>
                </div>
            </div>
        `;
        resultSection.style.display = 'block';
        resultSection.style.animation = 'bounceIn 0.8s ease-out';

        // Add sound effect to download button
        document.querySelector('.download-btn').addEventListener('click', playButtonSound);
    }

    function showError(message) {
        hideAllSections();
        errorMessage.textContent = message;
        errorSection.style.display = 'block';
        errorSection.style.animation = 'shake 0.5s ease-out';
        if (pollingInterval) {
            clearInterval(pollingInterval);
        }
    }

    function hideAllSections() {
        processingSection.style.display = 'none';
        resultSection.style.display = 'none';
        errorSection.style.display = 'none';
    }

    window.resetForm = function() {
        playButtonSound();
        hideAllSections();
        uploadForm.reset();
        if (pollingInterval) {
            clearInterval(pollingInterval);
        }
        currentRequestId = null;
    };

    // Add CSS animations dynamically
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInUp {
            from { transform: translateY(30px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
        
        @keyframes bounceIn {
            0% { transform: scale(0.3); opacity: 0; }
            50% { transform: scale(1.05); }
            70% { transform: scale(0.9); }
            100% { transform: scale(1); opacity: 1; }
        }
        
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-10px); }
            20%, 40%, 60%, 80% { transform: translateX(10px); }
        }
        
        .result-image {
            animation: imageGlow 2s ease-in-out infinite alternate;
        }
        
        @keyframes imageGlow {
            from { filter: drop-shadow(0 0 10px #00ff00); }
            to { filter: drop-shadow(0 0 20px #00ffff); }
        }
    `;
    document.head.appendChild(style);
});
