let map;
let markers = [];

function initMap() {
    // التأكد من وجود الـ map div
    const mapDiv = document.getElementById('map');
    if (!mapDiv) {
        console.error('Map container not found!');
        return;
    }

    // تهيئة الخريطة
    map = L.map('map').setView([30.0444, 31.2357], 6); // مركز افتراضي (القاهرة)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // تحميل الشحنات من shipmentData
    const shipmentData = document.querySelector('#shipmentData');
    if (!shipmentData) {
        console.error('Shipment data not found!');
        return;
    }
    const shipments = JSON.parse(shipmentData.textContent);
    console.log('Shipments loaded:', shipments);

    shipments.forEach(shipment => {
        if (shipment.locationLat && shipment.locationLng) {
            const marker = L.marker([shipment.locationLat, shipment.locationLng], {
                icon: L.icon({
                    iconUrl: shipment.status === 'Delivered' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png' :
                        shipment.status === 'In Transit' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png' :
                            shipment.status === 'Rejected' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png' :
                                'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41],
                    popupAnchor: [1, -34],
                    shadowSize: [41, 41]
                })
            }).addTo(map);
            marker.bindPopup(`<h5>${shipment.id}</h5><p>Product: ${shipment.productName}</p><p>Status: ${shipment.status}</p><p>Location: ${shipment.currentLocation}</p>`);
            markers.push(marker);
        } else {
            console.warn(`No location data for shipment ${shipment.id}`);
        }
    });
}

function updateMarker(shipmentId, newStatus) {
    const marker = markers.find(m => m._popup._content.includes(shipmentId));
    if (marker) {
        marker.setIcon(L.icon({
            iconUrl: newStatus === 'Delivered' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png' :
                newStatus === 'In Transit' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png' :
                    newStatus === 'Rejected' ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png' :
                        'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
            shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34],
            shadowSize: [41, 41]
        }));
    }
}

function acceptShipment(shipmentId) {
    console.log('acceptShipment called with shipmentId:', shipmentId);
    if (!shipmentId || shipmentId === '0' || shipmentId === 0) {
        alert('Invalid shipment ID: ' + shipmentId);
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    console.log('Token:', token);

    fetch(`/Distributor/AcceptShipment?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                return response.text().then(text => { throw new Error(`Request failed: ${response.status} - ${text}`); });
            }
            return response.json();
        })
        .then(data => {
            console.log('Response data:', data);
            if (data.success) {
                alert(data.message);
                const row = document.querySelector(`tr[data-shipment-id="${shipmentId}"]`);
                row.querySelector('td:nth-child(4)').textContent = 'In Transit';
                row.querySelector('td:last-child').innerHTML = '<span class="text-success">Accepted</span>';
                updateMarker(shipmentId, 'In Transit');
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while accepting the shipment: ' + error.message);
        });
}

function rejectShipment(shipmentId) {
    console.log('rejectShipment called with shipmentId:', shipmentId);
    if (!shipmentId || shipmentId === '0' || shipmentId === 0) {
        alert('Invalid shipment ID: ' + shipmentId);
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    console.log('Token:', token);

    fetch(`/Distributor/RejectShipment?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                return response.text().then(text => { throw new Error(`Request failed: ${response.status} - ${text}`); });
            }
            return response.json();
        })
        .then(data => {
            console.log('Response data:', data);
            if (data.success) {
                alert(data.message);
                const row = document.querySelector(`tr[data-shipment-id="${shipmentId}"]`);
                row.querySelector('td:nth-child(4)').textContent = 'Rejected';
                row.querySelector('td:last-child').innerHTML = '<span class="text-danger">Rejected</span>';
                updateMarker(shipmentId, 'Rejected');
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while rejecting the shipment: ' + error.message);
        });
}

function confirmDelivery(shipmentId) {
    console.log('confirmDelivery called with shipmentId:', shipmentId);
    if (!shipmentId || shipmentId === '0' || shipmentId === 0) {
        alert('Invalid shipment ID: ' + shipmentId);
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    console.log('Token:', token);

    fetch(`/Distributor/ConfirmDelivery?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                return response.text().then(text => { throw new Error(`Request failed: ${response.status} - ${text}`); });
            }
            return response.json();
        })
        .then(data => {
            console.log('Response data:', data);
            if (data.success) {
                alert(data.message);
                const row = document.querySelector(`tr[data-shipment-id="${shipmentId}"]`);
                row.querySelector('td:nth-child(4)').textContent = 'Delivered';
                row.querySelector('td:last-child').innerHTML = '<span class="text-success">Delivered</span>';
                updateMarker(shipmentId, 'Delivered');
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while confirming delivery: ' + error.message);
        });
}

// تشغيل الخريطة عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', initMap);