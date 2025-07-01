// Initialize SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/trackingHub", {
        withCredentials: true // تأكد إن الـ Cookie بيتبعت مع الطلب
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveLocationUpdate", (shipmentId, latitude, longitude) => {
    console.log(`Location updated for shipment ${shipmentId}: (${latitude}, ${longitude})`);
    const row = document.querySelector(`tr[data-shipment-id="${shipmentId}"] td:nth-child(5)`);
    if (row) {
        row.innerHTML = `(${latitude}, ${longitude}) <button class="btn btn-secondary btn-sm" onclick="showMap(${shipmentId}, ${latitude}, ${longitude})">View Map</button>`;
    }
});

connection.start()
    .then(() => {
        console.log("SignalR connected successfully");
        // انضم لمجموعات الشحنات اللي موجودة في الجدول
        const shipmentData = JSON.parse(document.getElementById("shipmentData").textContent);
        shipmentData.forEach(shipment => {
            connection.invoke("JoinShipmentGroup", shipment.id).catch(err => {
                console.error("Error joining shipment group:", err);
            });
        });
    })
    .catch(err => {
        console.error("SignalR Connection Error:", err);
        alert("Failed to connect to tracking hub. Please try again.");
    });

// Initialize Leaflet map
let map;
function showMap(shipmentId, latitude, longitude) {
    const mapModal = new bootstrap.Modal(document.getElementById("mapModal"));
    mapModal.show();

    if (!map) { // تصحيح: استخدام map بدل 纬度
        map = L.map("map").setView([latitude, longitude], 13);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);
    } else {
        map.setView([latitude, longitude], 13);
    }

    L.marker([latitude, longitude]).addTo(map)
        .bindPopup(`Shipment ${shipmentId}`)
        .openPopup();
}

function showUpdateLocationModal(shipmentId) {
    document.getElementById("updateShipmentId").value = shipmentId;
    const updateModal = new bootstrap.Modal(document.getElementById("updateLocationModal"));
    updateModal.show();
}

function getCurrentLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            document.getElementById("latitude").value = position.coords.latitude;
            document.getElementById("longitude").value = position.coords.longitude;
        }, err => {
            alert("Error getting current location: " + err.message);
        });
    } else {
        alert("Geolocation is not supported by this browser.");
    }
}

// Handle form submission
document.getElementById("updateLocationForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    const shipmentId = document.getElementById("updateShipmentId").value;
    const latitude = parseFloat(document.getElementById("latitude").value);
    const longitude = parseFloat(document.getElementById("longitude").value);

    if (!latitude || !longitude) {
        alert("Please enter valid latitude and longitude.");
        return;
    }

    try {
        const response = await fetch("/Driver/UpdateVehicleLocation", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            credentials: "include", // تأكد إن الـ Cookie بيتبعت مع الطلب
            body: JSON.stringify({ shipmentId, latitude, longitude })
        });

        const result = await response.json();
        if (response.ok && result.success) {
            alert("Location updated successfully!");
            const updateModal = bootstrap.Modal.getInstance(document.getElementById("updateLocationModal"));
            updateModal.hide();
            const row = document.querySelector(`tr[data-shipment-id="${shipmentId}"] td:nth-child(5)`);
            if (row) {
                row.innerHTML = `(${latitude}, ${longitude}) <button class="btn btn-secondary btn-sm" onclick="showMap(${shipmentId}, ${latitude}, ${longitude})">View Map</button>`;
            }
        } else {
            console.error("Error response:", result);
            alert("Error: " + (result.message || "Failed to update location."));
        }
    } catch (err) {
        console.error("Error updating location:", err);
        alert("An error occurred while updating the location.");
    }
});