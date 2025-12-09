// Function to download a file by creating a temporary anchor element
window.downloadFile = (url, fileName) => {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'document';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

