let map, marker, path;
let currentShipmentId;
const shipments = JSON.parse(document.getElementById('shipmentData').textContent);
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/trackingHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveLocationUpdate", (shipmentId, latitude, longitude) => {
    const shipmentRow = document.querySelector(`tr[data-shipment-id="${shipmentId}"]`);
    if (shipmentRow) {
        shipmentRow.querySelector('td:nth-child(5)').innerHTML = `(${latitude.toFixed(4)}, ${longitude.toFixed(4)}) <button class="btn btn-secondary btn-sm" onclick="showMap(${shipmentId}, ${latitude}, ${longitude})">View Map</button>`;
        if (map && currentShipmentId == shipmentId) {
            if (marker) map.removeLayer(marker);
            marker = L.marker([latitude, longitude]).addTo(map);
            path.addLatLng([latitude, longitude]);
            map.fitBounds(path.getBounds());
            marker.bindPopup(`Shipment ${shipmentId}: (${latitude.toFixed(4)}, ${longitude.toFixed(4)})`).openPopup();
        }
    }
});

connection.on("ReceiveNotification", (message, shipmentId) => {
    showMessage(message, "success");
    location.reload();
});

connection.start().then(() => {
    shipments.forEach(shipment => {
        if (shipment.id) {
            connection.invoke("JoinShipmentGroup", shipment.id).catch(err => console.error(`Error joining shipment group ${shipment.id}:`, err));
        }
    });
}).catch(err => {
    console.error("Error starting SignalR connection:", err);
    showMessage("Failed to connect to real-time tracking. Please refresh the page.", "error");
});

function showMap(shipmentId, latitude, longitude) {
    if (!latitude || !longitude) {
        showMessage("No valid location data available for this shipment.", "error");
        return;
    }

    currentShipmentId = shipmentId;
    $('#mapModal').modal('show');

    if (!map) {
        map = L.map('map').setView([latitude, longitude], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);
        path = L.polyline([], { color: 'blue' }).addTo(map);
    } else {
        map.setView([latitude, longitude], 13);
    }

    if (marker) map.removeLayer(marker);
    marker = L.marker([latitude, longitude]).addTo(map);
    path.addLatLng([latitude, longitude]);
    map.fitBounds(path.getBounds());
    marker.bindPopup(`Shipment ${shipmentId}: (${latitude.toFixed(4)}, ${longitude.toFixed(4)})`).openPopup();

    fetch(`/Distributor/GetLatestLocation?shipmentId=${shipmentId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                path.addLatLng([data.latitude, data.longitude]);
                map.fitBounds(path.getBounds());
            } else {
                showMessage(data.message || "No location data found.", "error");
            }
        })
        .catch(error => {
            console.error('Error fetching latest location:', error);
            showMessage("Error loading location data.", "error");
        });
}

function showAssignDriverModal(shipmentId) {
    document.getElementById('shipmentId').value = shipmentId;
    $('#assignDriverModal').modal('show');

    fetch(`/Distributor/GetDrivers`)
        .then(response => response.json())
        .then(drivers => {
            const driverSelect = document.getElementById('driverId');
            driverSelect.innerHTML = '<option value="">Select a driver</option>';
            drivers.forEach(driver => {
                const option = document.createElement('option');
                option.value = driver.id;
                option.text = `${driver.fullName} - ${driver.licenseNumber}`;
                driverSelect.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error fetching drivers:', error);
            showMessage("Error loading drivers.", "error");
        });
}

function acceptShipment(shipmentId) {
    fetch(`/Distributor/AcceptShipment?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            showMessage(data.message, data.success ? "success" : "error");
            if (data.success) location.reload();
        })
        .catch(error => {
            console.error('Error accepting shipment:', error);
            showMessage("Error accepting shipment.", "error");
        });
}

function rejectShipment(shipmentId) {
    fetch(`/Distributor/RejectShipment?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            showMessage(data.message, data.success ? "success" : "error");
            if (data.success) location.reload();
        })
        .catch(error => {
            console.error('Error rejecting shipment:', error);
            showMessage("Error rejecting shipment.", "error");
        });
}

function confirmDelivery(shipmentId) {
    fetch(`/Distributor/ConfirmDelivery?id=${shipmentId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            showMessage(data.message, data.success ? "success" : "error");
            if (data.success) location.reload();
        })
        .catch(error => {
            console.error('Error confirming delivery:', error);
            showMessage("Error confirming delivery.", "error");
        });
}

function showMessage(message, type) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `alert alert-${type === "success" ? "success" : "danger"}`;
    messageDiv.textContent = message;
    document.body.appendChild(messageDiv);
    setTimeout(() => messageDiv.remove(), 5000);
}