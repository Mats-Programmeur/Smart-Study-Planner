const form = document.getElementById("task-form");
const taskList = document.getElementById("task-list");
const feedback = document.getElementById("form-feedback");

document.addEventListener("DOMContentLoaded", () => {
    loadTasks();
    form.addEventListener("submit", handleSubmit);
});

async function loadTasks() {
    setFeedback("");

    try {
        const response = await fetch("/tasks");

        if (!response.ok) {
            throw new Error("Taken konden niet worden geladen.");
        }

        const tasks = await response.json();
        renderTasks(tasks);
    } catch (error) {
        renderTasks([]);
        setFeedback(error.message, true);
    }
}

async function handleSubmit(event) {
    event.preventDefault();

    const formData = new FormData(form);
    const title = formData.get("title")?.toString().trim() ?? "";
    const description = formData.get("description")?.toString().trim() ?? "";

    if (!title) {
        setFeedback("Vul eerst een titel in.", true);
        document.getElementById("title").focus();
        return;
    }

    setFeedback("Taak wordt opgeslagen...");

    try {
        const response = await fetch("/tasks", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                titel: title,
                beschrijving: description
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || "Taak kon niet worden opgeslagen.");
        }

        form.reset();
        setFeedback("Taak opgeslagen.");
        await loadTasks();
    } catch (error) {
        setFeedback(error.message, true);
    }
}

function renderTasks(tasks) {
    taskList.innerHTML = "";

    if (!tasks.length) {
        const emptyState = document.createElement("p");
        emptyState.className = "empty-state";
        emptyState.textContent = "Er zijn nog geen taken toegevoegd.";
        taskList.appendChild(emptyState);
        return;
    }

    for (const task of tasks) {
        const card = document.createElement("article");
        const title = document.createElement("h3");
        const description = document.createElement("p");

        card.className = "task-card";
        title.textContent = task.titel ?? "Zonder titel";
        description.textContent = task.beschrijving || "Geen beschrijving toegevoegd.";

        card.append(title, description);
        taskList.appendChild(card);
    }
}

function setFeedback(message, isError = false) {
    feedback.textContent = message;
    feedback.classList.toggle("is-error", isError);
}
