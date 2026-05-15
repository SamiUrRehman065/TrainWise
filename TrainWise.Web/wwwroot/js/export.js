// Download file utility for export functionality
window.downloadFile = function (data, filename) {
    const bytes = new Uint8Array(data);
    const blob = new Blob([bytes], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

window.downloadBinaryFile = function (data, filename, contentType) {
    const bytes = new Uint8Array(data);
    const blob = new Blob([bytes], { type: contentType || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Copy to clipboard utility
window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text).then(() => {
        return true;
    }).catch(() => {
        return false;
    });
};

// Export utility for CSV/JSON export
window.exportData = function (data, filename, format) {
    let content, mimeType;

    if (format === 'json') {
        content = JSON.stringify(data, null, 2);
        mimeType = 'application/json';
    } else if (format === 'csv') {
        content = convertToCSV(data);
        mimeType = 'text/csv;charset=utf-8;';
    }

    const link = document.createElement('a');
    link.setAttribute('href', 'data:' + mimeType + ',\ufeff' + encodeURIComponent(content));
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

function convertToCSV(data) {
    if (!Array.isArray(data) || data.length === 0) return '';

    const headers = Object.keys(data[0]);
    const rows = data.map(item => 
        headers.map(header => {
            const value = item[header];
            return typeof value === 'string' && value.includes(',') ? `"${value}"` : value;
        }).join(',')
    );

    return [headers.join(','), ...rows].join('\n');
}
