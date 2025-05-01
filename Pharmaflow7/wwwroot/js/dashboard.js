let dashboardData = {
    stock: 150,
    incoming: 20,
    outgoing: 10,
    shipments: [
        { id: "SH001", type: "Incoming", quantity: 10, date: "2025-03-18", status: "Delivered" },
        { id: "SH002", type: "Outgoing", quantity: 5, date: "2025-03-19", status: "In Transit" }
    ]
};

// Chart initialization
let inventoryChart;

function initializeChart() {
    const ctx = document.getElementById("inventoryChart").getContext("2d");
    inventoryChart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: ["Current Inventory", "Incoming", "Outgoing"],
            datasets: [{
                label: "Quantity",
                data: [dashboardData.stock, dashboardData.incoming, dashboardData.outgoing],
                backgroundColor: ["#1e90ff", "#4682b4", "#b0c4de"],
                borderWidth: 0,
                borderRadius: 5,
            }]
        },
        options: {
            scales: {
                y: { beginAtZero: true }
            },
            plugins: {
                legend: { display: false }
            }
        }
    });
}

function updateChart() {
    inventoryChart.data.datasets[0].data = [dashboardData.stock, dashboardData.incoming, dashboardData.outgoing];
    inventoryChart.update();
}

function updateOverview() {
    document.getElementById("stock-value").textContent = dashboardData.stock;
    document.getElementById("incoming-value").textContent = dashboardData.incoming;
    document.getElementById("outgoing-value").textContent = dashboardData.outgoing;
}

function updateShipmentsTable() {
    const tbody = document.getElementById("shipments-body");
    tbody.innerHTML = "";
    dashboardData.shipments.forEach(shipment => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${shipment.id}</td>
            <td>${shipment.type}</td>
            <td>${shipment.quantity}</td>
            <td>${shipment.date}</td>
            <td>${shipment.status}</td>
        `;
        tbody.appendChild(row);
    });
}

function showLoading() {
    document.getElementById("loading").style.display = "flex";
    setTimeout(() => {
        document.getElementById("loading").style.display = "none";
    }, 1000); // Simulate loading for 1 second
}

window.onload = function () {
    updateOverview();
    updateShipmentsTable();
    initializeChart();
};

document.getElementById("refresh-btn").addEventListener("click", () => {
    showLoading();
    setTimeout(() => {
        dashboardData.stock += Math.floor(Math.random() * 10);
        dashboardData.incoming += Math.floor(Math.random() * 5);
        dashboardData.outgoing += Math.floor(Math.random() * 3);
        updateOverview();
        updateChart();
        alert("Data refreshed!");
    }, 1000);
});

document.getElementById("add-shipment-btn").addEventListener("click", () => {
    showLoading();
    setTimeout(() => {
        const newShipment = {
            id: `SH${String(dashboardData.shipments.length + 1).padStart(3, "0")}`,
            type: Math.random() > 0.5 ? "Incoming" : "Outgoing",
            quantity: Math.floor(Math.random() * 20) + 1,
            date: new Date().toISOString().split("T")[0],
            status: "Pending"
        };
        dashboardData.shipments.push(newShipment);
        if (newShipment.type === "Incoming") dashboardData.incoming += newShipment.quantity;
        else dashboardData.outgoing += newShipment.quantity;
        dashboardData.stock = newShipment.type === "Incoming" ? dashboardData.stock + newShipment.quantity : dashboardData.stock - newShipment.quantity;
        updateOverview();
        updateShipmentsTable();
        updateChart();
        alert("New shipment added!");
    }, 1000);
});