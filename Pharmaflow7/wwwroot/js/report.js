document.getElementById('reportForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const companyName = document.getElementById('companyName').value;
    const issueType = document.getElementById('issueType').value;
    const details = document.getElementById('details').value;

    const reportDiv = document.createElement('div');
    reportDiv.classList.add('report-item');
    reportDiv.innerHTML = `
        <strong>Company Name:</strong> ${companyName} <br>
        <strong>Issue Type:</strong> ${issueType} <br>
        <strong>Details:</strong> ${details}
    `;

    document.getElementById('reportList').appendChild(reportDiv);

    this.reset();

    alert('Report submitted successfully!');
});