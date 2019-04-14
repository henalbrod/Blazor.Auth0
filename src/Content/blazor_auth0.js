window.isLoggedIn = () => localStorage.getItem('isLoggedIn') === 'true';
window.setIsLoggedIn = (val) => localStorage.setItem('isLoggedIn', val);
window.drawAuth0Iframe = (instance, src) => {
    let iframe = document.createElement('iframe');
    iframe.setAttribute('src', src);
    iframe.style.display = "none";
    document.body.appendChild(iframe);
    window.addEventListener('message', (msg) => {
        if (msg.data.type == "authorization_response") {
            instance.invokeMethodAsync('HandleAuth0Message', {
                isTrusted: msg.isTrusted,
                origin: msg.origin,
                type: msg.data.type,
                code: msg.data.response.code,
                state: msg.data.response.state,
                error: msg.data.response.error,
                errorDescription: msg.data.response.error_description
            }).then((r) => {document.body.removeChild(iframe);});
        }
    });
};