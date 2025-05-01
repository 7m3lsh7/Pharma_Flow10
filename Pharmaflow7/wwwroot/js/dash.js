 
const salesCtx = document.getElementById('salesChart');
if (salesCtx) {
    new Chart(salesCtx.getContext('2d'), {
        type: 'line',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            datasets: [{
                label: 'Sales (Units)',
                data: [120, 150, 180, 170, 200, 220],
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

 
const distributionCtx = document.getElementById('distributionChart');
if (distributionCtx) {
    new Chart(distributionCtx.getContext('2d'), {
        type: 'pie',
        data: {
            labels: ['Cairo', 'Alexandria', 'Giza', 'Others'],
            datasets: [{
                label: 'Distribution',
                data: [40, 30, 20, 10],
                backgroundColor: ['#2d89ef', '#66ccff', '#1e70bf', '#55b2e8']
            }]
        },
        options: {
            responsive: true
        }
    });
}