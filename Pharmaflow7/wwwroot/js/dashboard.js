document.addEventListener('DOMContentLoaded', () => {
    const loadingOverlay = document.getElementById('loading');
    const refreshBtn = document.getElementById('refresh-btn');
    const addShipmentBtn = document.getElementById('add-shipment-btn');
    const addShipmentModal = document.getElementById('add-shipment-modal');
    const modalClose = document.querySelector('.modal-close');
    const addShipmentForm = document.getElementById('add-shipment-form');
    const stockValue = document.getElementById('stock-value');
    const incomingValue = document.getElementById('incoming-value');
    const outgoingValue = document.getElementById('outgoing-value');
    const shipmentsBody = document.getElementById('shipments-body');

    let inventoryChart;

    // جلب بيانات الـ Dashboard
    async function fetchDashboardData() {
        try {
            loadingOverlay.style.display = 'flex';
            const response = await fetch('/Distributor/GetDashboardData', {
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            stockValue.textContent = data.inventoryCount ?? '0';
            incomingValue.textContent = data.incomingShipments ?? '0';
            outgoingValue.textContent = data.outgoingShipments ?? '0';
        } catch (error) {
            console.error('Error fetching dashboard data:', error);
            stockValue.textContent = 'Error';
            incomingValue.textContent = 'Error';
            outgoingValue.textContent = 'Error';
        } finally {
            loadingOverlay.style.display = 'none';
        }
    }

    // جلب بيانات الـ Shipments
    async function fetchShipments() {
        try {
            loadingOverlay.style.display = 'flex';
            const response = await fetch('/Distributor/GetShipments', {
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const shipments = await response.json();
            shipmentsBody.innerHTML = '';
            if (shipments.length === 0) {
                shipmentsBody.innerHTML = '<tr><td colspan="5">No shipments found.</td></tr>';
            } else {
                shipments.forEach(s => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${s.id}</td>
                        <td>${s.type}</td>
                        <td>${s.quantity}</td>
                        <td>${s.date}</td>
                        <td>${s.status}</td>
                    `;
                    shipmentsBody.appendChild(row);
                });
            }
            updateChart(shipments);
        } catch (error) {
            console.error('Error fetching shipments:', error);
            shipmentsBody.innerHTML = '<tr><td colspan="5">Error loading shipments.</td></tr>';
        } finally {
            loadingOverlay.style.display = 'none';
        }
    }

    // تحديث الـ Chart
    function updateChart(shipments) {
        const ctx = document.getElementById('inventoryChart').getContext('2d');
        const statusCounts = {
            'Pending': 0,
            'In Transit': 0,
            'Delivered': 0,
            'Rejected': 0
        };

        shipments.forEach(s => {
            if (statusCounts[s.status]) {
                statusCounts[s.status]++;
            }
        });

        if (inventoryChart) {
            inventoryChart.destroy();
        }

        inventoryChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: Object.keys(statusCounts),
                datasets: [{
                    label: 'Shipments by Status',
                    data: Object.values(statusCounts),
                    backgroundColor: ['#005f99', '#00b4d8', '#4caf50', '#f44336'],
                    borderColor: ['#003d66', '#0088a8', '#388e3c', '#d32f2f'],
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Number of Shipments'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Status'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
    }

    // فتح/إغلاق الـ Modal
    addShipmentBtn.addEventListener('click', () => {
        addShipmentModal.style.display = 'flex';
    });

    modalClose.addEventListener('click', () => {
        addShipmentModal.style.display = 'none';
    });

    addShipmentModal.addEventListener('click', (e) => {
        if (e.target === addShipmentModal) {
            addShipmentModal.style.display = 'none';
        }
    });

    // معالجة Form إضافة Shipment
    addShipmentForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(addShipmentForm);
        const shipmentData = {
            productName: formData.get('productName'),
            quantity: parseInt(formData.get('quantity')),
            destination: formData.get('destination')
        };

        try {
            loadingOverlay.style.display = 'flex';
            // TODO: استبدلي الـ endpoint ده لو عندك action فعلية لإضافة shipment
            const response = await fetch('/Distributor/CreateShipment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(shipmentData)
            });
            const result = await response.json();
            if (result.success) {
                alert(result.message || 'Shipment created successfully!');
                addShipmentModal.style.display = 'none';
                fetchShipments();
                fetchDashboardData();
            } else {
                alert(result.message || 'Failed to create shipment.');
            }
        } catch (error) {
            console.error('Error creating shipment:', error);
            alert('An error occurred while creating the shipment.');
        } finally {
            loadingOverlay.style.display = 'none';
        }
    });

    // Refresh Data
    refreshBtn.addEventListener('click', () => {
        fetchDashboardData();
        fetchShipments();
    });

    // جلب البيانات عند تحميل الصفحة
    fetchDashboardData();
    fetchShipments();
});