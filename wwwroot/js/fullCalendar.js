document.addEventListener('DOMContentLoaded', function () {
    const calendarEl = document.getElementById('calendar');
    const userCanManage = window.userRoles.canManage;

    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        height: 'auto',
        selectable: true,
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        eventSources: [{
            url: '/EventSchedules/GetEvents',
            method: 'GET'
        }],
        dateClick: function (info) {
            if (userCanManage) {
                // Redirect to create form with pre-filled date
                const startDate = info.dateStr;
                window.location.href = `/EventSchedules/Create?date=${startDate}`;
            } else {
                alert("You don't have permission to add events.");
            }
        },
        eventClick: function (info) {
            // Redirect to details page when event is clicked
            window.location.href = `/EventSchedules/Details/${info.event.id}`;
        },
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        }
    });

    calendar.render();
});