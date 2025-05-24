// Create star background
function createStars() {
    const stars = document.querySelector('.stars');
    const starCount = 100;

    for (let i = 0; i < starCount; i++) {
        const star = document.createElement('div');
        star.style.position = 'absolute';
        star.style.width = `${Math.random() * 3 + 1}px`;
        star.style.height = star.style.width;
        star.style.background = 'white';
        star.style.borderRadius = '50%';
        star.style.top = `${Math.random() * 100}%`;
        star.style.left = `${Math.random() * 100}%`;
        star.style.opacity = Math.random() * 0.8 + 0.2;
        star.style.animation = `twinkle ${Math.random() * 5 + 2}s infinite alternate`;
        stars.appendChild(star);
    }

    // Add twinkling animation
    const style = document.createElement('style');
    style.textContent = `
        @keyframes twinkle {
            0% { opacity: 0.2; transform: scale(0.8); }
            100% { opacity: 1; transform: scale(1); }
        }
    `;
    document.head.appendChild(style);
}

// Uptime counter
function startUptimeCounter() {
    const uptimeElement = document.getElementById('uptime');
    if (!uptimeElement) return;

    const startTime = new Date();

    setInterval(() => {
        const now = new Date();
        const diff = Math.floor((now - startTime) / 1000);
        const minutes = Math.floor(diff / 60);
        const seconds = diff % 60;

        uptimeElement.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }, 1000);
}

// Test server connection
function initTestButton() {
    const button = document.getElementById('test-server');
    if (!button) return;

    button.addEventListener('click', () => {
        const originalText = button.textContent;

        button.textContent = 'Testing... 🔄';
        button.disabled = true;

        // Update connection counter
        const connectionsEl = document.getElementById('connections');
        if (connectionsEl) {
            const currentCount = parseInt(connectionsEl.textContent);
            connectionsEl.textContent = currentCount + 1;
        }

        // Update response time
        const responseEl = document.getElementById('response');
        if (responseEl) {
            const responseTime = Math.floor(Math.random() * 40) + 5;
            responseEl.textContent = `${responseTime}ms`;
        }

        // Simulate server test
        setTimeout(() => {
            button.textContent = 'Connected! ✅';
            button.style.background = '#4ecdc4';

            setTimeout(() => {
                button.textContent = originalText;
                button.disabled = false;
                button.style.background = '';
            }, 2000);
        }, 800);
    });
}

// Animate data packets
function animatePackets() {
    const packets = document.querySelectorAll('.packet');

    function animatePacket(packet, delay) {
        setTimeout(() => {
            packet.style.opacity = '1';
            packet.style.transform = 'translateX(-40px)';
            packet.style.transition = 'all 1.2s ease-out';

            setTimeout(() => {
                packet.style.opacity = '0';
                packet.style.transform = 'translateX(0)';

                setTimeout(() => {
                    packet.style.transition = 'none';
                    packet.style.transform = 'translateX(0)';
                    animatePacket(packet, Math.random() * 1500 + 500);
                }, 1200);
            }, 1200);
        }, delay);
    }

    packets.forEach((packet, index) => {
        animatePacket(packet, index * 400);
    });
}

// Smooth scrolling for navigation
function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();

            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                window.scrollTo({
                    top: target.offsetTop - 80,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    createStars();
    startUptimeCounter();
    initTestButton();
    animatePackets();
    initSmoothScroll();

    console.log('🚀 TCP Web Server interface loaded!');
});