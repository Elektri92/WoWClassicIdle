// Notifies a .NET component whenever the browser tab becomes visible or hidden.
window.pageVisibility = {
    register: function (dotNetRef) {
        const handler = () => dotNetRef.invokeMethodAsync('OnVisibilityChanged', !document.hidden);
        document.addEventListener('visibilitychange', handler);
        return {
            dispose: () => document.removeEventListener('visibilitychange', handler)
        };
    },
    isVisible: function () {
        return !document.hidden;
    }
};
