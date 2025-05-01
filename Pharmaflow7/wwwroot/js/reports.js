// wwwroot/js/scripts.js
// Sample data (to be replaced with real data from Backend)
const salesData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    data: [120, 150, 180, 170, 200, 220]
};

const issues = [
    { id: 'ISS001', product: 'Paracetamol 500mg', type: 'Stock Shortage', reportedBy: 'Cairo Dist.', date: '2025-03-10', status: 'Pending' },
    { id: 'ISS002', product: 'Amoxicillin 250mg', type: 'Delivery Delay', reportedBy: 'Alexandria Pharmacy', date: '2025-03-12', status: 'Resolved' },
    { id: 'ISS003', product: 'Ibuprofen 400mg', type: 'Quality Issue', reportedBy: 'Giza Dist.', date: '2025-03-15', status: 'In Progress' }
];

const distributionData = {
    labels: ['Cairo', 'Alexandria', 'Giza', 'Others'],
    data: [40, 30, 20, 10]
};

const topProductsData = {
    labels: ['Paracetamol 500mg', 'Amoxicillin 250mg', 'Ibuprofen 400mg'],
    data: [50, 30, 20]
};

// Load issues into the table
function loadIssues() {
    const tbody = document.getElementById('issuesTableBody');
    if (tbody) {
        tbody.innerHTML = '';
        issues.forEach(issue => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${issue.id}</td>
                <td>${issue.product}</td>
                <td>${issue.type}</td>
                <td>${issue.reportedBy}</td>
                <td>${issue.date}</td>
                <td><span class="badge ${issue.status === 'Pending' ? 'bg-warning' : issue.status === 'Resolved' ? 'bg-success' : 'bg-info'}">${issue.status}</span></td>
            `;
            tbody.appendChild(row);
        });
    }
}

// Sales Chart
const salesCtx = document.getElementById('salesChart');
if (salesCtx) {
    new Chart(salesCtx.getContext('2d'), {
        type: 'line',
        data: {
            labels: salesData.labels,
            datasets: [{
                label: 'Sales (Units)',
                data: salesData.data,
                borderColor: '#66ccff',
                backgroundColor: 'rgba(102, 204, 255, 0.2)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            scales: { y: { beginAtZero: true } }
        }
    });
}

// Distribution Chart
const distributionCtx = document.getElementById('distributionChart');
if (distributionCtx) {
    new Chart(distributionCtx.getContext('2d'), {
        type: 'pie',
        data: {
            labels: distributionData.labels,
            datasets: [{
                label: 'Distribution',
                data: distributionData.data,
                backgroundColor: ['#2d89ef', '#66ccff', '#1e70bf', '#55b2e8']
            }]
        },
        options: {
            responsive: true
        }
    });
}

// Top Products Chart
const topProductsCtx = document.getElementById('topProductsChart');
if (topProductsCtx) {
    new Chart(topProductsCtx.getContext('2d'), {
        type: 'bar',
        data: {
            labels: topProductsData.labels,
            datasets: [{
                label: 'Units Sold',
                data: topProductsData.data,
                backgroundColor: '#66ccff'
            }]
        },
        options: {
            responsive: true,
            scales: { y: { beginAtZero: true } }
        }
    });
}

// Initial load
loadIssues();