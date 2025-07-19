document.addEventListener("DOMContentLoaded", () => {
    const reportForm = document.getElementById("reportForm");
    const reportList = document.getElementById("reportList");

    // Load monthly chart
    function loadMonthlyChart() {
        fetch("/Distributor/GetMonthlyReport")
            .then(response => response.json())
            .then(data => {
                const ctx = document.getElementById("monthlyChart").getContext("2d");
                new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: data.labels,
                        datasets: [{
                            label: "Shipments by Status",
                            data: data.values,
                            backgroundColor: ["#007bff", "#28a745", "#dc3545", "#ffc107"],
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: { position: "top" }
                        }
                    }
                });
            })
            .catch(error => console.error("Error loading chart:", error));
    }

    // Load reports
    function loadReports() {
        fetch("/Distributor/GetReports")
            .then(response => response.json())
            .then(data => {
                reportList.innerHTML = "";
                data.forEach(report => {
                    const row = document.createElement("tr");
                    row.innerHTML = `
                        <td>${report.companyName}</td>
                        <td>${report.issueType}</td>
                        <td>${report.details}</td>
                        <td>${report.date}</td>
                    `;
                    reportList.appendChild(row);
                });
            })
            .catch(error => console.error("Error loading reports:", error));
    }

    // Submit report
    reportForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const formData = new FormData(reportForm);
        const data = {
            companyName: formData.get("companyName"),
            issueType: formData.get("issueType"),
            details: formData.get("details")
        };

        fetch("/Distributor/SubmitReport", {
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
                    reportForm.reset();
                    loadReports();
                }
                alert(result.message);
            })
            .catch(error => console.error("Error submitting report:", error));
    });

    // Initial load
    loadMonthlyChart();
    loadReports();
});