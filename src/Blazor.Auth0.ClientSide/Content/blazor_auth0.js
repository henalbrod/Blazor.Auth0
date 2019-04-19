window.isLoggedIn = () => localStorage.getItem('isLoggedIn') === 'true';
window.setIsLoggedIn = (val) => localStorage.setItem('isLoggedIn', val);
window.getNonce = () => localStorage.getItem('nonce');
window.setNonce = (val) => localStorage.setItem('nonce', val);
window.clearNonce = (val) => localStorage.removeItem('nonce');
window.drawAuth0Iframe = (instance, src) => {
    let iframe = document.createElement('iframe');
    iframe.setAttribute('src', src);
    iframe.style.display = "none";
    document.body.appendChild(iframe);
    var messageListener = (msg) => {
        if (msg.data.type == "authorization_response") {
            window.removeEventListener('message', messageListener);
            instance.invokeMethodAsync('HandleAuth0Message', {
                isTrusted: msg.isTrusted,
                origin: msg.origin,
                type: msg.data.type,
                state: msg.data.response.state,
                error: msg.data.response.error,
                errorDescription: msg.data.response.error_description,
                // Code Grant (Recommended)
                code: msg.data.response.code,
                // Implicit Grant (Legacy)
                accessToken: msg.data.response.access_token,
                idToken: msg.data.response.id_token,
                scope: msg.data.response.scope,
                tokenType: msg.data.response.token_type,
                expiresIn: msg.data.response.expires_in
            }).then((r) => { document.body.removeChild(iframe); });
        }
    };
    window.addEventListener('message', messageListener);
};