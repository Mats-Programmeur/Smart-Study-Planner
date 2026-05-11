const state = {
    token: localStorage.getItem("ssp_token"),
    user: null,
    authMode: null,
    tasks: [],
    deadlines: [],
    users: []
};

const authChoice = document.getElementById("auth-choice");
const authForm = document.getElementById("auth-form");
const authFormTitle = document.getElementById("auth-form-title");
const authNameField = document.getElementById("auth-name-field");
const authPasswordInput = document.getElementById("auth-password");
const authSubmitButton = document.getElementById("auth-submit-button");
const authSwitchButton = document.getElementById("auth-switch-button");
const taskForm = document.getElementById("task-form");
const deadlineForm = document.getElementById("deadline-form");
const logoutButton = document.getElementById("logout-button");
const taskCancelButton = document.getElementById("task-cancel");
const deadlineCancelButton = document.getElementById("deadline-cancel");
const authFeedback = document.getElementById("auth-feedback");
const taskFeedback = document.getElementById("task-feedback");
const deadlineFeedback = document.getElementById("deadline-feedback");
const adminFeedback = document.getElementById("admin-feedback");
const studentDashboard = document.getElementById("student-dashboard");
const adminDashboard = document.getElementById("admin-dashboard");
const sessionCard = document.getElementById("session-card");
const sessionName = document.getElementById("session-name");
const sessionMeta = document.getElementById("session-meta");
const taskList = document.getElementById("task-list");
const deadlineList = document.getElementById("deadline-list");
const planningList = document.getElementById("planning-list");
const adviceList = document.getElementById("advice-list");
const userList = document.getElementById("user-list");

document.addEventListener("DOMContentLoaded", async () => {
    bindEvents();
    setDateDefaults();
    updateDashboardVisibility();

    if (state.token) {
        await restoreSession();
    }
});

function bindEvents() {
    authChoice.addEventListener("click", handleAuthChoiceClick);
    authForm.addEventListener("submit", handleAuthSubmit);
    authSwitchButton.addEventListener("click", handleAuthSwitchClick);
    taskForm.addEventListener("submit", handleTaskSubmit);
    deadlineForm.addEventListener("submit", handleDeadlineSubmit);
    logoutButton.addEventListener("click", logout);
    taskCancelButton.addEventListener("click", resetTaskForm);
    deadlineCancelButton.addEventListener("click", resetDeadlineForm);
    taskList.addEventListener("click", handleTaskListClick);
    deadlineList.addEventListener("click", handleDeadlineListClick);
    userList.addEventListener("click", handleUserListClick);
}

function handleAuthChoiceClick(event) {
    const button = event.target.closest("button[data-auth-mode]");

    if (!button) {
        return;
    }

    setAuthMode(button.dataset.authMode);
}

function handleAuthSwitchClick() {
    setAuthMode(state.authMode === "register" ? "login" : "register");
}

async function restoreSession() {
    try {
        state.user = await apiFetch("/auth/me");
        setFeedback(authFeedback, "");
        updateDashboardVisibility();
        await loadDashboards();
    } catch (error) {
        logout();
        setFeedback(authFeedback, "Sessie verlopen. Log opnieuw in.", true);
    }
}

async function handleAuthSubmit(event) {
    event.preventDefault();

    const action = state.authMode ?? "login";
    const formData = new FormData(authForm);
    const payload = {
        naam: formData.get("naam")?.toString().trim() ?? "",
        email: formData.get("email")?.toString().trim() ?? "",
        wachtwoord: formData.get("wachtwoord")?.toString() ?? ""
    };

    if (!payload.email || !payload.wachtwoord) {
        setFeedback(authFeedback, "Vul e-mailadres en wachtwoord in.", true);
        return;
    }

    if (action === "register" && !payload.naam) {
        setFeedback(authFeedback, "Vul ook een naam in voor een nieuw account.", true);
        return;
    }

    setFeedback(authFeedback, action === "register" ? "Account wordt aangemaakt..." : "Je wordt ingelogd...");

    try {
        const endpoint = action === "register" ? "/auth/register" : "/auth/login";
        const body = action === "register" ? payload : { email: payload.email, wachtwoord: payload.wachtwoord };
        const response = await apiFetch(endpoint, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        }, false);

        state.token = response.token;
        state.user = {
            naam: response.naam,
            email: response.email,
            rol: response.rol
        };

        localStorage.setItem("ssp_token", state.token);
        authForm.reset();
        setDateDefaults();
        setFeedback(authFeedback, action === "register" ? "Account aangemaakt en ingelogd." : "Inloggen gelukt.");
        updateDashboardVisibility();
        await loadDashboards();
    } catch (error) {
        setFeedback(authFeedback, error.message, true);
    }
}

function logout() {
    state.token = null;
    state.user = null;
    state.authMode = null;
    state.tasks = [];
    state.deadlines = [];
    state.users = [];

    localStorage.removeItem("ssp_token");

    authForm.reset();
    resetTaskForm();
    resetDeadlineForm();
    setDateDefaults();
    taskList.innerHTML = "";
    deadlineList.innerHTML = "";
    planningList.innerHTML = "";
    adviceList.innerHTML = "";
    userList.innerHTML = "";
    setFeedback(authFeedback, "");
    setFeedback(taskFeedback, "");
    setFeedback(deadlineFeedback, "");
    setFeedback(adminFeedback, "");
    updateDashboardVisibility();
}

async function loadDashboards() {
    if (!state.user) {
        return;
    }

    if (state.user.rol === "Student") {
        await Promise.all([
            loadTasks(),
            loadDeadlines(),
            loadPlanning(),
            loadAdvice()
        ]);
    }

    if (state.user.rol === "Beheerder") {
        await loadUsers();
    }
}

async function loadTasks() {
    state.tasks = await apiFetch("/tasks");
    renderTasks();
}

async function loadDeadlines() {
    state.deadlines = await apiFetch("/deadlines");
    renderDeadlines();
}

async function loadPlanning() {
    const planning = await apiFetch("/planning");
    renderPlanning(planning);
}

async function loadAdvice() {
    const advice = await apiFetch("/advice");
    renderAdvice(advice);
}

async function loadUsers() {
    state.users = await apiFetch("/users");
    renderUsers();
}

async function handleTaskSubmit(event) {
    event.preventDefault();

    const formData = new FormData(taskForm);
    const taskId = document.getElementById("task-id").value;
    const payload = {
        titel: formData.get("titel")?.toString().trim() ?? "",
        beschrijving: formData.get("beschrijving")?.toString().trim() ?? "",
        datum: formData.get("datum")?.toString() ?? "",
        startTijd: formData.get("startTijd")?.toString() ?? "",
        eindTijd: formData.get("eindTijd")?.toString() ?? "",
        prioriteit: formData.get("prioriteit")?.toString() ?? "Normaal",
        geschatteStudietijdMinuten: Number(formData.get("geschatteStudietijdMinuten") ?? 60),
        status: formData.get("status")?.toString() ?? "Gepland"
    };

    if (!payload.titel || !payload.datum || !payload.startTijd || !payload.eindTijd) {
        setFeedback(taskFeedback, "Vul alle verplichte taakvelden in.", true);
        return;
    }

    if (payload.eindTijd <= payload.startTijd) {
        setFeedback(taskFeedback, "Eindtijd moet later zijn dan starttijd.", true);
        return;
    }

    const method = taskId ? "PUT" : "POST";
    const endpoint = taskId ? `/tasks/${taskId}` : "/tasks";
    setFeedback(taskFeedback, taskId ? "Taak wordt bijgewerkt..." : "Taak wordt opgeslagen...");

    try {
        await apiFetch(endpoint, {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        setFeedback(taskFeedback, taskId ? "Taak bijgewerkt." : "Taak opgeslagen.");
        resetTaskForm();
        await Promise.all([loadTasks(), loadPlanning(), loadAdvice()]);
    } catch (error) {
        setFeedback(taskFeedback, error.message, true);
    }
}

async function handleDeadlineSubmit(event) {
    event.preventDefault();

    const formData = new FormData(deadlineForm);
    const deadlineId = document.getElementById("deadline-id").value;
    const payload = {
        titel: formData.get("titel")?.toString().trim() ?? "",
        beschrijving: formData.get("beschrijving")?.toString().trim() ?? "",
        datum: formData.get("datum")?.toString() ?? "",
        eindTijd: formData.get("eindTijd")?.toString() ?? "",
        prioriteit: formData.get("prioriteit")?.toString() ?? "Normaal"
    };

    if (!payload.titel || !payload.datum || !payload.eindTijd) {
        setFeedback(deadlineFeedback, "Vul alle verplichte deadlinevelden in.", true);
        return;
    }

    const method = deadlineId ? "PUT" : "POST";
    const endpoint = deadlineId ? `/deadlines/${deadlineId}` : "/deadlines";
    setFeedback(deadlineFeedback, deadlineId ? "Deadline wordt bijgewerkt..." : "Deadline wordt opgeslagen...");

    try {
        await apiFetch(endpoint, {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        setFeedback(deadlineFeedback, deadlineId ? "Deadline bijgewerkt." : "Deadline opgeslagen.");
        resetDeadlineForm();
        await Promise.all([loadDeadlines(), loadPlanning(), loadAdvice()]);
    } catch (error) {
        setFeedback(deadlineFeedback, error.message, true);
    }
}

async function handleTaskListClick(event) {
    const button = event.target.closest("button[data-task-action]");

    if (!button) {
        return;
    }

    const taskId = Number(button.dataset.taskId);
    const task = state.tasks.find(item => item.id === taskId);

    if (!task) {
        return;
    }

    if (button.dataset.taskAction === "edit") {
        document.getElementById("task-id").value = task.id;
        document.getElementById("task-title").value = task.titel;
        document.getElementById("task-description").value = task.beschrijving ?? "";
        document.getElementById("task-date").value = task.datum;
        document.getElementById("task-start-time").value = task.startTijd.slice(0, 5);
        document.getElementById("task-end-time").value = task.eindTijd.slice(0, 5);
        document.getElementById("task-priority").value = task.prioriteit;
        document.getElementById("task-estimate").value = task.geschatteStudietijdMinuten;
        document.getElementById("task-status").value = task.status;
        setFeedback(taskFeedback, "Taak geladen om te bewerken.");
        document.getElementById("task-title").focus();
        return;
    }

    if (button.dataset.taskAction === "delete") {
        try {
            await apiFetch(`/tasks/${taskId}`, { method: "DELETE" });
            setFeedback(taskFeedback, "Taak verwijderd.");
            resetTaskForm();
            await Promise.all([loadTasks(), loadPlanning(), loadAdvice()]);
        } catch (error) {
            setFeedback(taskFeedback, error.message, true);
        }
    }
}

async function handleDeadlineListClick(event) {
    const button = event.target.closest("button[data-deadline-action]");

    if (!button) {
        return;
    }

    const deadlineId = Number(button.dataset.deadlineId);
    const deadline = state.deadlines.find(item => item.id === deadlineId);

    if (!deadline) {
        return;
    }

    if (button.dataset.deadlineAction === "edit") {
        document.getElementById("deadline-id").value = deadline.id;
        document.getElementById("deadline-title").value = deadline.titel;
        document.getElementById("deadline-description").value = deadline.beschrijving ?? "";
        document.getElementById("deadline-date").value = deadline.datum;
        document.getElementById("deadline-time").value = deadline.eindTijd.slice(0, 5);
        document.getElementById("deadline-priority").value = deadline.prioriteit;
        setFeedback(deadlineFeedback, "Deadline geladen om te bewerken.");
        document.getElementById("deadline-title").focus();
        return;
    }

    if (button.dataset.deadlineAction === "delete") {
        try {
            await apiFetch(`/deadlines/${deadlineId}`, { method: "DELETE" });
            setFeedback(deadlineFeedback, "Deadline verwijderd.");
            resetDeadlineForm();
            await Promise.all([loadDeadlines(), loadPlanning(), loadAdvice()]);
        } catch (error) {
            setFeedback(deadlineFeedback, error.message, true);
        }
    }
}

async function handleUserListClick(event) {
    const button = event.target.closest("button[data-user-id]");

    if (!button) {
        return;
    }

    const userId = Number(button.dataset.userId);
    const form = button.closest("form");
    const formData = new FormData(form);
    const payload = {
        rol: formData.get("rol")?.toString() ?? "Student",
        isActief: formData.get("isActief") === "on"
    };

    setFeedback(adminFeedback, "Gebruiker wordt bijgewerkt...");

    try {
        await apiFetch(`/users/${userId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        setFeedback(adminFeedback, "Gebruiker bijgewerkt.");
        await loadUsers();
    } catch (error) {
        setFeedback(adminFeedback, error.message, true);
    }
}

function renderTasks() {
    taskList.innerHTML = "";

    if (!state.tasks.length) {
        taskList.appendChild(createEmptyState("Er zijn nog geen taken toegevoegd voor deze student."));
        return;
    }

    for (const task of state.tasks) {
        taskList.appendChild(createItemCard({
            title: task.titel,
            meta: [
                formatDate(task.datum),
                `${task.startTijd.slice(0, 5)} - ${task.eindTijd.slice(0, 5)}`,
                `${task.geschatteStudietijdMinuten} min`,
                task.prioriteit,
                task.status
            ],
            description: task.beschrijving,
            actions: `
                <button type="button" class="ghost-button" data-task-action="edit" data-task-id="${task.id}">Bewerken</button>
                <button type="button" class="danger-button" data-task-action="delete" data-task-id="${task.id}">Verwijderen</button>
            `
        }));
    }
}

function renderDeadlines() {
    deadlineList.innerHTML = "";

    if (!state.deadlines.length) {
        deadlineList.appendChild(createEmptyState("Er zijn nog geen deadlines toegevoegd voor deze student."));
        return;
    }

    for (const deadline of state.deadlines) {
        deadlineList.appendChild(createItemCard({
            title: deadline.titel,
            meta: [
                formatDate(deadline.datum),
                deadline.eindTijd.slice(0, 5),
                deadline.prioriteit
            ],
            description: deadline.beschrijving,
            actions: `
                <button type="button" class="ghost-button" data-deadline-action="edit" data-deadline-id="${deadline.id}">Bewerken</button>
                <button type="button" class="danger-button" data-deadline-action="delete" data-deadline-id="${deadline.id}">Verwijderen</button>
            `
        }));
    }
}

function renderPlanning(planning) {
    planningList.innerHTML = "";

    const grouped = new Map();

    for (const task of planning.taken ?? []) {
        if (!grouped.has(task.datum)) {
            grouped.set(task.datum, { taken: [], deadlines: [] });
        }
        grouped.get(task.datum).taken.push(task);
    }

    for (const deadline of planning.deadlines ?? []) {
        if (!grouped.has(deadline.datum)) {
            grouped.set(deadline.datum, { taken: [], deadlines: [] });
        }
        grouped.get(deadline.datum).deadlines.push(deadline);
    }

    if (!grouped.size) {
        planningList.appendChild(createEmptyState("Er is nog geen planning opgebouwd."));
        return;
    }

    const sortedDates = [...grouped.keys()].sort();

    for (const date of sortedDates) {
        const day = grouped.get(date);
        const card = document.createElement("article");
        card.className = "planning-card";

        const tasksMarkup = day.taken.length
            ? day.taken.map(task => `<li>Taak: ${escapeHtml(task.titel)} (${task.startTijd.slice(0, 5)} - ${task.eindTijd.slice(0, 5)})</li>`).join("")
            : "<li>Geen taken</li>";
        const deadlinesMarkup = day.deadlines.length
            ? day.deadlines.map(deadline => `<li>Deadline: ${escapeHtml(deadline.titel)} (${deadline.eindTijd.slice(0, 5)})</li>`).join("")
            : "<li>Geen deadlines</li>";

        card.innerHTML = `
            <h3>${formatDate(date)}</h3>
            <div class="planning-columns">
                <div>
                    <p class="muted-text">Taken</p>
                    <ul>${tasksMarkup}</ul>
                </div>
                <div>
                    <p class="muted-text">Deadlines</p>
                    <ul>${deadlinesMarkup}</ul>
                </div>
            </div>
        `;

        planningList.appendChild(card);
    }
}

function renderAdvice(advice) {
    adviceList.innerHTML = "";

    if (!advice.length) {
        adviceList.appendChild(createEmptyState("De huidige planning oogt realistisch. Er is geen extra advies nodig."));
        return;
    }

    for (const item of advice) {
        const card = document.createElement("article");
        card.className = "item-card advice-card";
        card.innerHTML = `
            <div class="item-header">
                <h3>${escapeHtml(item.type)} - ${escapeHtml(item.ernstniveau)}</h3>
            </div>
            <p>${escapeHtml(item.melding)}</p>
            <p class="muted-text">${escapeHtml(item.aanbeveling)}</p>
        `;
        adviceList.appendChild(card);
    }
}

function renderUsers() {
    userList.innerHTML = "";

    if (!state.users.length) {
        userList.appendChild(createEmptyState("Er zijn geen gebruikers gevonden."));
        return;
    }

    for (const user of state.users) {
        const wrapper = document.createElement("article");
        wrapper.className = "item-card";
        wrapper.innerHTML = `
            <div class="item-header">
                <div>
                    <h3>${escapeHtml(user.naam)}</h3>
                    <p class="muted-text">${escapeHtml(user.email)}</p>
                </div>
                <span class="badge">${escapeHtml(user.rol)}</span>
            </div>
            <form class="user-form">
                <div class="form-row compact-row">
                    <div class="form-field">
                        <label>Rol</label>
                        <select name="rol">
                            <option value="Student" ${user.rol === "Student" ? "selected" : ""}>Student</option>
                            <option value="Beheerder" ${user.rol === "Beheerder" ? "selected" : ""}>Beheerder</option>
                        </select>
                    </div>
                    <label class="checkbox-field">
                        <input type="checkbox" name="isActief" ${user.isActief ? "checked" : ""}>
                        <span>Actief</span>
                    </label>
                </div>
                <button type="button" class="primary-button" data-user-id="${user.id}">Wijzig opslaan</button>
            </form>
        `;
        userList.appendChild(wrapper);
    }
}

function updateDashboardVisibility() {
    const isLoggedIn = Boolean(state.user);
    const isStudent = state.user?.rol === "Student";
    const isAdmin = state.user?.rol === "Beheerder";

    authChoice.classList.toggle("is-hidden", isLoggedIn || Boolean(state.authMode));
    authForm.classList.toggle("is-hidden", isLoggedIn || !state.authMode);
    sessionCard.classList.toggle("is-hidden", !isLoggedIn);
    studentDashboard.classList.toggle("is-hidden", !isStudent);
    adminDashboard.classList.toggle("is-hidden", !isAdmin);

    if (isLoggedIn) {
        sessionName.textContent = state.user.naam;
        sessionMeta.textContent = `${state.user.email} | ${state.user.rol}`;
    } else {
        sessionName.textContent = "-";
        sessionMeta.textContent = "";
    }
}

function setAuthMode(mode) {
    state.authMode = mode;

    const isRegister = mode === "register";
    authFormTitle.textContent = isRegister ? "Account aanmaken" : "Inloggen";
    authNameField.classList.toggle("is-hidden", !isRegister);
    authPasswordInput.autocomplete = isRegister ? "new-password" : "current-password";
    authSubmitButton.textContent = isRegister ? "Account maken" : "Inloggen";
    authSwitchButton.textContent = isRegister ? "Ik wil toch inloggen" : "Ik wil toch een account aanmaken";
    setFeedback(authFeedback, "");
    updateDashboardVisibility();
    document.getElementById(isRegister ? "auth-name" : "auth-email").focus();
}

function resetTaskForm() {
    taskForm.reset();
    document.getElementById("task-id").value = "";
    document.getElementById("task-estimate").value = 60;
    document.getElementById("task-priority").value = "Normaal";
    document.getElementById("task-status").value = "Gepland";
    document.getElementById("task-date").value = todayString();
    setFeedback(taskFeedback, "");
}

function resetDeadlineForm() {
    deadlineForm.reset();
    document.getElementById("deadline-id").value = "";
    document.getElementById("deadline-priority").value = "Normaal";
    document.getElementById("deadline-date").value = todayString();
    setFeedback(deadlineFeedback, "");
}

function setDateDefaults() {
    const today = todayString();
    document.getElementById("task-date").value = document.getElementById("task-date").value || today;
    document.getElementById("deadline-date").value = document.getElementById("deadline-date").value || today;
}

function createEmptyState(message) {
    const element = document.createElement("p");
    element.className = "empty-state";
    element.textContent = message;
    return element;
}

function createItemCard({ title, meta, description, actions }) {
    const card = document.createElement("article");
    card.className = "item-card";
    card.innerHTML = `
        <div class="item-header">
            <div>
                <h3>${escapeHtml(title)}</h3>
                <p class="muted-text">${meta.map(escapeHtml).join(" | ")}</p>
            </div>
        </div>
        <p>${escapeHtml(description || "Geen beschrijving toegevoegd.")}</p>
        <div class="action-row">${actions}</div>
    `;
    return card;
}

async function apiFetch(url, options = {}, requiresAuth = true) {
    const headers = new Headers(options.headers ?? {});

    if (requiresAuth && state.token) {
        headers.set("Authorization", `Bearer ${state.token}`);
    }

    const response = await fetch(url, {
        ...options,
        headers
    });

    const errorText = response.ok || response.status === 204
        ? ""
        : (await response.text()).trim();

    if (response.status === 401) {
        throw new Error(errorText || "E-mailadres of wachtwoord klopt niet.");
    }

    if (response.status === 403) {
        throw new Error("Deze functionaliteit hoort bij een andere rol.");
    }

    if (!response.ok) {
        throw new Error(errorText || "Er ging iets mis tijdens het laden.");
    }

    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get("Content-Type") ?? "";
    if (!contentType.includes("application/json")) {
        return null;
    }

    return await response.json();
}

function setFeedback(element, message, isError = false) {
    element.textContent = message;
    element.classList.toggle("is-error", isError);
}

function formatDate(value) {
    const date = new Date(`${value}T00:00:00`);
    return new Intl.DateTimeFormat("nl-NL", {
        weekday: "short",
        day: "numeric",
        month: "short"
    }).format(date);
}

function todayString() {
    return new Date().toISOString().split("T")[0];
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}
