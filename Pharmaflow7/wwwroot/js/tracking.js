// Sample shipment data (replace with backend API call)
const shipments = [
    { id: 'SH12345', product: 'Paracetamol 500mg', destination: 'Cairo Pharmacy', status: 'In Transit', location: { lat: 30.0444, lng: 31.2357 }, currentLocation: 'Cairo' },
    { id: 'SH12346', product: 'Amoxicillin 250mg', destination: 'Alexandria Dist.', status: 'Delivered', location: { lat: 31.2001, lng: 29.9187 }, currentLocation: 'Alexandria' },
    { id: 'SH12347', product: 'Ibuprofen 400mg', destination: 'Giza Pharmacy', status: 'Processing', location: { lat: 30.0131, lng: 31.2089 }, currentLocation: 'Giza' }
];

// Load shipments into the table
function loadShipments(shipmentData) {
    const tbody = document.getElementById('shipmentTableBody');
    tbody.innerHTML = '';
    shipmentData.forEach(shipment => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${shipment.id}</td>
            <td>${shipment.product}</td>
            <td>${shipment.destination}</td>
            <td><span class="badge ${shipment.status === 'In Transit' ? 'bg-warning' : shipment.status === 'Delivered' ? 'bg-success' : 'bg-info'}">${shipment.status}</span></td>
            <td>${shipment.currentLocation}</td>
            <td><button class="btn btn-primary btn-sm" onclick="showOnMap(${shipment.location.lat}, ${shipment.location.lng}, '${shipment.id}')">View on Map</button></td>
        `;
        tbody.appendChild(row);
    });
}

// Initialize Google Map
let map;
let markers = [];
function initMap() {
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: 30.0444, lng: 31.2357 }, // Default center (Cairo)
        zoom: 6
    });

    // Add markers for all shipments
    shipments.forEach(shipment => {
        const marker = new google.maps.Marker({
            position: shipment.location,
            map: map,
            title: `${shipment.id} - ${shipment.product}`,
            icon: shipment.status === 'Delivered' ? 'http://maps.google.com/mapfiles/ms/icons/green-dot.png' :
                shipment.status === 'In Transit' ? 'http://maps.google.com/mapfiles/ms/icons/yellow-dot.png' :
                    'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
        });
        markers.push(marker);

        const infoWindow = new google.maps.InfoWindow({
            content: `<h5>${shipment.id}</h5><p>Product: ${shipment.product}</p><p>Status: ${shipment.status}</p><p>Location: ${shipment.currentLocation}</p>`
        });
        marker.addListener('click', () => {
            infoWindow.open(map, marker);
        });
    });
}

// Show specific shipment on map
function showOnMap(lat, lng, shipmentId) {
    map.setCenter({ lat, lng });
    map.setZoom(12);
    markers.forEach(marker => {
        if (marker.title.includes(shipmentId)) {
            new google.maps.InfoWindow({
                content: marker.title
            }).open(map, marker);
        }
    });
}

// Fetch shipments from backend (example)
async function fetchShipments() {
    try {
        const response = await fetch('/api/shipments'); // Replace with your backend endpoint
        const shipmentData = await response.json();
        loadShipments(shipmentData); // Load dynamic data
    } catch (error) {
        console.error('Error fetching shipments:', error);
        loadShipments(shipments); // Fallback to static data
    }
}

// Initial load
fetchShipments();