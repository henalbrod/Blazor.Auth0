"use strict";

if (window.opener && window.name === "auth0_signup_popup") {
    window.opener.___blazor_auth0.popupCallback(window.location.href);
    close();
}

window.___blazor_auth0 = {
    logOut: (src) => {
        "use strict";
        return new Promise((resolve) => {
            let iframe = document.createElement("iframe");
            iframe.setAttribute("src", src);
            iframe.style.display = "none";
            document.body.appendChild(iframe);
            iframe.onload = () => {                
                document.body.removeChild(iframe);
                resolve();
            };
        });
    },
    drawIframe: (instance, src) => {
        "use strict";
        let iframe = document.createElement("iframe");
        iframe.setAttribute("src", src);
        iframe.style.display = "none";
        document.body.appendChild(iframe);
        var messageListener = (msg) => {
            if (msg.data.type === "authorization_response") {
                window.removeEventListener("message", messageListener);
                instance.invokeMethodAsync("HandleAuthorizationResponseAsync", {
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
        window.addEventListener("message", messageListener);
    },
    popupLogin: (instance, src) => {
        "use strict";        

        let top = 100;
        let left = (window.innerWidth / 2) - 225;

        window.___blazor_auth0.popupCallback = (path) => {
            instance.invokeMethodAsync("ValidateSession", path)
                .then((r) => {
                    window.___blazor_auth0.popupCallback = null;
                });
        };

        let popup = window.open(src, "auth0_signup_popup", "width=450,height=700,top=" + top + ",left=" + left + ",menubar=no,location=no,resizable=no,scrollbars=no,status=no,personalbar=no");
        popup.focus();

    }
};