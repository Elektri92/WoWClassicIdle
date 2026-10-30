// WebKitGTK cancels a drag immediately when the drag data is empty. Drag state lives on the server,
// so non-image draggables (e.g. character chips) carry no data of their own – add a placeholder.
// A custom type keeps the placeholder from being pasted into text inputs.
document.addEventListener('dragstart', function (e) {
    if (e.dataTransfer && e.dataTransfer.types.length === 0) {
        e.dataTransfer.setData('application/x-wow-idle-drag', '1');
    }
});
