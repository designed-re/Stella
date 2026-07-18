// Minimal helpers: CSRF token fetch + a small emit() wrapper for plugin pages.
window.Stella = window.Stella || {};
Stella.csrf = function () {
    const t = document.querySelector('input[name="__RequestVerificationToken"]');
    return t ? t.value : "";
};
Stella.emit = async function (pluginId, eventName, payload) {
    const res = await fetch(`/webui/api/emit/${pluginId}/${eventName}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": Stella.csrf(),
        },
        body: JSON.stringify(payload || {}),
    });
    if (res.status === 204) return null;
    const ct = res.headers.get("content-type") || "";
    return ct.includes("application/json") ? res.json() : res.text();
};
Stella.emitForm = async function (pluginId, eventName, form) {
    const body = new FormData(form);
    const res = await fetch(`/webui/api/emit/${pluginId}/${eventName}`, {
        method: "POST",
        headers: { "X-CSRF-TOKEN": Stella.csrf() },
        body: body,
    });
    if (res.status === 204) return null;
    const ct = res.headers.get("content-type") || "";
    return ct.includes("application/json") ? res.json() : res.text();
};
