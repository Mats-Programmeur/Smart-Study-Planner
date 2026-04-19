const form = document.getElementById("task-form");
const taskList = document.getElementById("task-list");
const feedback = document.getElementById("form-feedback");

document.addEventListener("DOMContentLoaded", () => {
    setDateConstraints();
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
    const date = formData.get("date")?.toString() ?? "";
    const startTime = formData.get("startTime")?.toString() ?? "";
    const endTime = formData.get("endTime")?.toString() ?? "";

    if (!title) {
        setFeedback("Vul eerst een titel in.", true);
        document.getElementById("title").focus();
        return;
    }

    if (!date || !startTime || !endTime) {
        setFeedback("Vul datum, starttijd en eindtijd in.", true);
        return;
    }

    if (date < getEarliestAllowedDate()) {
        setFeedback("Je kunt een taak niet verder dan 1 dag terug plannen.", true);
        document.getElementById("date").focus();
        return;
    }

    if (endTime <= startTime) {
        setFeedback("De eindtijd moet later zijn dan de starttijd.", true);
        document.getElementById("end-time").focus();
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
                beschrijving: description,
                datum: date,
                startTijd: startTime,
                eindTijd: endTime
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
        description.textContent = buildTaskSummary(task);

        card.append(title, description);
        taskList.appendChild(card);
    }
}

function setFeedback(message, isError = false) {
    feedback.textContent = message;
    feedback.classList.toggle("is-error", isError);
}

function buildTaskSummary(task) {
    const details = [];

    if (task.datum) {
        details.push(formatDate(task.datum));
    }

    if (task.startTijd && task.eindTijd) {
        details.push(`${task.startTijd.slice(0, 5)} - ${task.eindTijd.slice(0, 5)}`);
    }

    if (task.beschrijving) {
        details.push(task.beschrijving);
    }

    return details.join(" | ") || "Geen beschrijving toegevoegd.";
}

function formatDate(value) {
    const date = new Date(`${value}T00:00:00`);

    return new Intl.DateTimeFormat("nl-NL", {
        weekday: "short",
        day: "numeric",
        month: "short"
    }).format(date);
}

function setDateConstraints() {
    const dateInput = document.getElementById("date");
    const earliestAllowedDate = getEarliestAllowedDate();

    dateInput.min = earliestAllowedDate;

    if (!dateInput.value) {
        dateInput.value = new Date().toISOString().split("T")[0];
    }
}

function getEarliestAllowedDate() {
    const earliestDate = new Date();
    earliestDate.setDate(earliestDate.getDate() - 1);
    return earliestDate.toISOString().split("T")[0];
}
