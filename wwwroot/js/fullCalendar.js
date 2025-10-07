document.addEventListener('DOMContentLoaded', function () {
    const calendarEl = document.getElementById('calendar');
    const modalEl = document.getElementById('eventModal');
    const modalTitle = document.getElementById('eventTitle');
    const modalBody = document.getElementById('eventBody');
    const btnDelete = document.getElementById('btnDelete');
    const btnEdit = document.getElementById('btnEdit');
    const userCanManage = window.userRoles.canManage === "true";
    const modal = new bootstrap.Modal(modalEl);

    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        height: 'auto',
        selectable: true,
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: ''
        },
        eventSources: [{
            url: '/EventSchedules/GetEvents',
            method: 'GET'
        }],
        dateClick: function (info) {
            if (userCanManage) openCreatePrompt(info.dateStr);
            else alert("You don't have permission to add events.");
        },
        eventClick: function (info) {
            openEventModal(info.event);
        }
    });

    calendar.render();

    function openEventModal(event) {
        modalTitle.innerText = event.title;
        modalBody.innerHTML = `
            <p><strong>Start:</strong> ${event.startStr}</p>
            <p><strong>End:</strong> ${event.endStr ?? 'N/A'}</p>
            <p><strong>Description:</strong> ${event.extendedProps.description ?? 'No description'}</p>
        `;
        modal.show();

        if (btnDelete) {
            btnDelete.onclick = async function () {
                if (confirm("Are you sure you want to delete this event?")) {
                    await fetch('/EventSchedules/' + event.id, { method: 'DELETE' });
                    calendar.refetchEvents();
                    modal.hide();
                }
            };
        }

        if (btnEdit) {
            btnEdit.onclick = function () {
                openEditPrompt(event);
            };
        }
    }

    function openCreatePrompt(dateStr) {
        const title = prompt("Enter event title for " + dateStr + ":");
        if (!title) return;

        const description = prompt("Enter event description:") || "";
        const eventData = {
            title: title,
            start: dateStr + "T09:00:00",
            end: dateStr + "T12:00:00",
            description: description,
            isAllDay: false
        };

        fetch('/EventSchedules/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(eventData)
        }).then(() => calendar.refetchEvents());
    }

    function openEditPrompt(event) {
        const newTitle = prompt("Edit title:", event.title);
        if (!newTitle) return;

        const newDesc = prompt("Edit description:", event.extendedProps.description ?? "");
        const start = event.startStr;
        const end = event.endStr;

        const updatedEvent = {
            id: event.id,
            title: newTitle,
            start: start,
            end: end,
            description: newDesc,
            isAllDay: false
        };

        fetch('/EventSchedules/' + event.id, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedEvent)
        }).then(() => {
            calendar.refetchEvents();
            modal.hide();
        });
    }
});
