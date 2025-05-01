let inventoryData = {
    items: [
        { name: "Paracetamol", warehouse: "WH1", quantity: 150, expiry: "2025-12-31", status: "Available" },
        { name: "Ibuprofen", warehouse: "WH2", quantity: 20, expiry: "2024-06-15", status: "Low Stock" },
        { name: "Aspirin", warehouse: "WH1", quantity: 5, expiry: "2023-03-10", status: "Expired" }
    ]
};


Chart.register(ChartDataLabels);

// Chart initialization
let stockChart;

function initializeChart() {
    const ctx = document.getElementById("stockChart").getContext("2d");
    const totalItems = inventoryData.items.length;
    const lowStock = inventoryData.items.filter(item => item.status === "Low Stock").length;
    const expired = inventoryData.items.filter(item => item.status === "Expired").length;
    const available = totalItems - lowStock - expired;

  
    const total = available + lowStock + expired;
    const availablePercent = ((available / total) * 100).toFixed(1);
    const lowStockPercent = ((lowStock / total) * 100).toFixed(1);
    const expiredPercent = ((expired / total) * 100).toFixed(1);

    stockChart = new Chart(ctx, {
        type: "pie",
        data: {
            labels: ["Available", "Low Stock", "Expired"],
            datasets: [{
                data: [available, lowStock, expired],
                backgroundColor: ["#4CAF50", "#FFC107", "#FF5722"],
                borderWidth: 2,
                borderColor: "#fff",
                hoverOffset: 20,
             
                shadowOffsetX: 3,
                shadowOffsetY: 3,
                shadowBlur: 10,
                shadowColor: "rgba(0, 0, 0, 0.3)"
            }]
        },
        options: {
            plugins: {
                legend: {
                    position: "bottom",
                    labels: {
                        font: {
                            size: 14,
                            family: "'Poppins', Arial, sans-serif",
                            weight: "500"
                        },
                        padding: 30,
                        boxWidth: 20,
                        color: "#333",
                       
                        generateLabels: (chart) => {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: label,
                                fillStyle: data.datasets[0].backgroundColor[i],
                                strokeStyle: "#fff",
                                lineWidth: 2,
                                fontColor: "#333",
                                padding: 10,
                                borderRadius: 5
                            }));
                        }
                    }
                },
                tooltip: {
                    backgroundColor: "rgba(0, 0, 0, 0.8)",
                    titleFont: { size: 14, family: "'Poppins', Arial, sans-serif" },
                    bodyFont: { size: 12, family: "'Poppins', Arial, sans-serif" },
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            const label = context.label || '';
                            const value = context.raw || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ${value} items (${percentage}%)`;
                        }
                    }
                },
                datalabels: {
                    color: "#fff",
                    font: {
                        size: 12,
                        family: "'Poppins', Arial, sans-serif",
                        weight: "bold"
                    },
                    formatter: (value, context) => {
                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                        const percentage = ((value / total) * 100).toFixed(1);
                        return value > 0 ? `${percentage}%` : '';
                    },
                    textShadow: "0 0 5px rgba(0, 0, 0, 0.5)",
                    anchor: "center",
                    align: "center",
                    offset: 10
                }
            },
            responsive: true,
            maintainAspectRatio: true,
            layout: {
                padding: 10
            },
            animation: {
                animateScale: true,
                animateRotate: true,
                duration: 1000,
                easing: "easeInOutQuart"
            },
           
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    const label = stockChart.data.labels[index];
                    filterTableByStatus(label);
                }
            }
        }
    });
}

function updateChart() {
    const totalItems = inventoryData.items.length;
    const lowStock = inventoryData.items.filter(item => item.status === "Low Stock").length;
    const expired = inventoryData.items.filter(item => item.status === "Expired").length;
    const available = totalItems - lowStock - expired;

    stockChart.data.datasets[0].data = [available, lowStock, expired];
    stockChart.update();
}

function updateOverview() {
    const totalItems = inventoryData.items.length;
    const lowStock = inventoryData.items.filter(item => item.status === "Low Stock").length;
    const expired = inventoryData.items.filter(item => item.status === "Expired").length;

    document.getElementById("total-items-value").textContent = totalItems;
    document.getElementById("low-stock-value").textContent = lowStock;
    document.getElementById("expired-value").textContent = expired;
}

function updateInventoryTable() {
    const tbody = document.getElementById("inventory-body");
    tbody.innerHTML = "";
    inventoryData.items.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${item.name}</td>
            <td>${item.warehouse}</td>
            <td>${item.quantity}</td>
            <td>${item.expiry}</td>
            <td>${item.status}</td>
        `;
        tbody.appendChild(row);
    });
}

function filterTableByStatus(status) {
    const tbody = document.getElementById("inventory-body");
    tbody.innerHTML = "";
    const filteredItems = inventoryData.items.filter(item => item.status === status);
    filteredItems.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${item.name}</td>
            <td>${item.warehouse}</td>
            <td>${item.quantity}</td>
            <td>${item.expiry}</td>
            <td>${item.status}</td>
        `;
        tbody.appendChild(row);
    });
  
    document.querySelector(".inventory-table h2").textContent = `${status} Medicines`;
}

function showLoading() {
    document.getElementById("loading").style.display = "flex";
    setTimeout(() => {
        document.getElementById("loading").style.display = "none";
    }, 1000);
}


window.onload = function () {
    updateOverview();
    updateInventoryTable();
    initializeChart();
};


document.getElementById("refresh-btn").addEventListener("click", () => {
    showLoading();
    setTimeout(() => {
        inventoryData.items.forEach(item => {
            item.quantity += Math.floor(Math.random() * 10);
            item.status = item.quantity <= 10 ? "Low Stock" : "Available";
            const today = new Date().toISOString().split("T")[0];
            if (item.expiry < today) item.status = "Expired";
        });
        updateOverview();
        updateInventoryTable();
        updateChart();
        document.querySelector(".inventory-table h2").textContent = "Available Medicines";
        alert("Data refreshed!");
    }, 1000);
});

document.getElementById("add-item-btn").addEventListener("click", () => {
    showLoading();
    setTimeout(() => {
        const newItem = {
            name: `Medicine ${inventoryData.items.length + 1}`,
            warehouse: `WH${Math.floor(Math.random() * 3) + 1}`,
            quantity: Math.floor(Math.random() * 200) + 1,
            expiry: "2026-01-01",
            status: "Available"
        };
        inventoryData.items.push(newItem);
        updateOverview();
        updateInventoryTable();
        updateChart();
        document.querySelector(".inventory-table h2").textContent = "Available Medicines";
        alert("New item added!");
    }, 1000);
});