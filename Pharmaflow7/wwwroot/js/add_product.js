document.getElementById('addProductForm').addEventListener('submit', function (event) {
    event.preventDefault();

    const formData = {
        Name: document.getElementById('productName').value.trim(),
        ProductionDate: document.getElementById('productionDate').value,
        ExpirationDate: document.getElementById('expirationDate').value,
        Description: document.getElementById('description').value.trim()
    };

    if (!formData.Name) {
        alert('Please fill in the Name field (required).');
        return;
    }
    if (!formData.ProductionDate || !formData.ExpirationDate) {
        alert('Please fill in Production Date and Expiration Date.');
        return;
    }
    if (new Date(formData.ProductionDate) >= new Date(formData.ExpirationDate)) {
        alert('Expiration date must be after production date.');
        return;
    }

    console.log('Sending data:', JSON.stringify(formData)); // تسجيل البيانات المُرسلة

    fetch('/Company/AddProduct', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(formData)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(`Failed to add product: ${response.status} - ${text}`); });
            }
            return response.json();
        })
        .then(data => {
            const qrData = JSON.stringify({
                id: data.productId,
                name: formData.Name,
                productionDate: formData.ProductionDate,
                expirationDate: formData.ExpirationDate,
                description: formData.Description
            });

            const qrCodeContainer = document.getElementById('qrCodeContainer');
            const qrCodeDiv = document.getElementById('qrCode');
            qrCodeDiv.innerHTML = '';
            new QRCode(qrCodeDiv, {
                text: qrData,
                width: 200,
                height: 200,
                colorDark: "#000000",
                colorLight: "#ffffff"
            });

            qrCodeContainer.style.display = 'block';
            alert('Product added successfully! QR Code generated.');
        })
        .catch(error => {
            console.error('Error:', error);
            alert(`An error occurred while adding the product: ${error.message}`);
        });
});

function downloadQR() {
    const qrCanvas = document.querySelector('#qrCode canvas');
    if (qrCanvas) {
        const link = document.createElement('a');
        link.download = 'product-qr-code.png';
        link.href = qrCanvas.toDataURL('image/png');
        link.click();
    }
}