document.addEventListener("DOMContentLoaded", () => {
    const loading = document.getElementById("loading");
    const refreshBtn = document.getElementById("refresh-btn");
    const addShipmentBtn = document.getElementById("add-shipment-btn");
    const modal = document.getElementById("add-shipment-modal");
    const closeModal = document.querySelector(".modal-close");
    const addShipmentForm = document.getElementById("add-shipment-form");

    // Load dashboard data
    function loadDashboardData() {
        loading.style.display = "flex";
        fetch("/Distributor/GetDashboardData")
            .then(response => response.json())
            .then(data => {
                document.getElementById("drivers-value").textContent = data.driversCount || 0;
                document.getElementById("stock-value").textContent = data.inventoryCount || 0;
                document.getElementById("incoming-value").textContent = data.incomingShipments || 0;
                document.getElementById("outgoing-value").textContent = data.outgoingShipments || 0;
                loading.style.display = "none";
            })
            .catch(error => {
                console.error("Error loading dashboard data:", error);
                loading.style.display = "none";
            });
    }

    // Load shipments
    function loadShipments() {
        fetch("/Distributor/GetShipments")
            .then(response => response.json())
            .then(data => {
                const tbody = document.getElementById("shipments-body");
                tbody.innerHTML = "";
                data.forEach(shipment => {
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

                // Update chart
                const ctx = document.getElementById("shipmentsChart").getContext("2d");
                const statusCounts = data.reduce((acc, s) => {
                    acc[s.status] = (acc[s.status] || 0) + 1;
                    return acc;
                }, {});
                new Chart(ctx, {
                    type: "bar",
                    data: {
                        labels: Object.keys(statusCounts),
                        datasets: [{
                            label: "Shipments by Status",
                            data: Object.values(statusCounts),
                            backgroundColor: ["#007bff", "#28a745", "#dc3545", "#ffc107"],
                        }]
                    },
                    options: {
                        responsive: true,
                        scales: {
                            y: { beginAtZero: true }
                        }
                    }
                });
            })
            .catch(error => console.error("Error loading shipments:", error));
    }

    // Modal handling
    addShipmentBtn.addEventListener("click", () => {
        modal.style.display = "flex";
        // Load products dynamically
        fetch("/Distributor/GetProducts")
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById("product-id");
                select.innerHTML = '<option value="">Select Product</option>';
                data.forEach(product => {
                    const option = document.createElement("option");
                    option.value = product.id;
                    option.textContent = product.name;
                    select.appendChild(option);
                });
            });
    });

    closeModal.addEventListener("click", () => {
        modal.style.display = "none";
    });

    addShipmentForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const formData = new FormData(addShipmentForm);
        const data = {
            productId: formData.get("productId"),
          
            destination: formData.get("destination")
        };

        fetch("/Distributor/CreateShipment", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    modal.style.display = "none";
                    loadShipments();
                    loadDashboardData();
                } else {
                    alert(result.message);
                }
            })
            .catch(error => console.error("Error creating shipment:", error));
    });

    // Accept/Reject shipment
    window.acceptShipment = (id) => {
        fetch(`/Distributor/AcceptShipment?id=${id}`, {
            method: "POST",
            headers: {
                "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
            }
        })
            .then(response => response.json())
            .then(result => {
                alert(result.message);
                loadShipments();
            });
    };

    window.rejectShipment = (id) => {
        fetch(`/Distributor/RejectShipment?id=${id}`, {
            method: "POST",
            headers: {
                "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").value
            }
        })
            .then(response => response.json())
            .then(result => {
                alert(result.message);
                loadShipments();
            });
    };

    refreshBtn.addEventListener("click", () => {
        loadDashboardData();
        loadShipments();
    });

    // Initial load
    loadDashboardData();
    loadShipments();
});