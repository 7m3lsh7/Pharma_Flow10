const scanBtn = document.getElementById('scanBtn');
const scannerContainer = document.getElementById('scanner-container');
const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const result = document.getElementById('result');
const closeBtn = document.getElementById('closeBtn');
const ctx = canvas.getContext('2d');

//  Scan
scanBtn.addEventListener('click', () => {
    scannerContainer.classList.remove('scanner-hidden');
    navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } })
        .then(stream => {
            video.srcObject = stream;
            video.play();
            scan();
        })
        .catch(err => {
            result.textContent = 'Error: ' + err.message;
            console.error(err); 
        });
});


closeBtn.addEventListener('click', () => {
    scannerContainer.classList.add('scanner-hidden');
    if (video.srcObject) {
        video.srcObject.getTracks()[0].stop();
    }
    result.textContent = 'Scanning for QR Code...';
});


function scan() {
    if (!video.srcObject) return;
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const code = jsQR(imageData.data, imageData.width, imageData.height);

    if (code) {
        result.textContent = 'QR Code Found: ' + code.data;
        video.srcObject.getTracks()[0].stop();
    } else {
        result.textContent = 'No QR Code detected yet...';
        requestAnimationFrame(scan); 
    }
}


window.addEventListener('scroll', () => {
    const sections = document.querySelectorAll('.features, .how-it-works, .statistics, .testimonials');
    sections.forEach(section => {
        const position = section.getBoundingClientRect().top;
        const screenPosition = window.innerHeight / 1.3;

        if (position < screenPosition) {
            section.style.opacity = '1';
            section.style.transform = 'translateY(0)';
        }
    });
});