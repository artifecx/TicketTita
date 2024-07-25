document.addEventListener('DOMContentLoaded', function () {
    const dashboardElement = document.getElementById('dashboard-data');
    const dashboardData = {
        openTickets: dashboardElement.dataset.openTickets,
        inProgressTickets: dashboardElement.dataset.inProgressTickets,
        unassignedTickets: dashboardElement.dataset.unassignedTickets,
        newTickets: dashboardElement.dataset.newTickets,
        completedTickets: dashboardElement.dataset.completedTickets,
        averageResolutionTime: dashboardElement.dataset.averageResolutionTime,
        averageFeedbackRating: dashboardElement.dataset.averageFeedbackRating,
        feedbacksCount: dashboardElement.dataset.feedbacksCount,
        totalTicketsSoftware: dashboardElement.dataset.totalTicketsSoftware,
        totalTicketsHardware: dashboardElement.dataset.totalTicketsHardware,
        totalTicketsNetwork: dashboardElement.dataset.totalTicketsNetwork,
        totalTicketsAccount: dashboardElement.dataset.totalTicketsAccount,
        totalTicketsOther: dashboardElement.dataset.totalTicketsOther,
        feedbackRatings: JSON.parse(dashboardElement.dataset.feedbackRatings),
        ticketDatesCreated: JSON.parse(dashboardElement.dataset.ticketDatesCreated || '[]'),
        ticketDatesResolved: JSON.parse(dashboardElement.dataset.ticketDatesResolved || '[]')
    };

    const labels = [];
    const createdData = new Array(7).fill(0);
    const resolvedData = new Array(7).fill(0);
    const currentDate = new Date();

    for (let i = 6; i >= 0; i--) {
        const date = new Date(currentDate);
        date.setDate(currentDate.getDate() - i);
        const formattedDate = date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
        labels.push(formattedDate);
    }

    dashboardData.ticketDatesCreated.forEach(dateString => {
        const date = new Date(dateString);
        const index = 6 - Math.floor((currentDate - date) / (1000 * 60 * 60 * 24));
        if (index >= 0 && index < 7) {
            createdData[index]++;
        }
    });

    dashboardData.ticketDatesResolved.forEach(dateString => {
        const date = new Date(dateString);
        const index = 6 - Math.floor((currentDate - date) / (1000 * 60 * 60 * 24));
        if (index >= 0 && index < 7) {
            resolvedData[index]++;
        }
    });

    const ctxBarLine = document.getElementById('bar-chart').getContext('2d');
    const dataBarLine = {
        labels: labels,
        datasets: [
            {
                type: 'bar',
                label: 'Tickets Created',
                data: createdData,
                backgroundColor: 'rgba(5, 37, 66, 1)',
                order: 1
            },
            {
                type: 'line',
                label: 'Tickets Resolved',
                data: resolvedData,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 2,
                fill: false,
                tension: 0.4,
                pointBackgroundColor: 'rgba(75, 192, 192, 1)',
                pointRadius: 4,
                order: 0
            }
        ]
    };

    const optionsBarLine = {
        scales: {
            y: {
                beginAtZero: true,
                max: Math.max(...createdData, ...resolvedData) + 10
            }
        },
        plugins: {
            legend: {
                display: false,
                position: 'top',
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        let label = context.dataset.label || '';
                        label += ': ' + context.raw;
                        return label;
                    }
                }
            }
        }
    };

    new Chart(ctxBarLine, {
        data: dataBarLine,
        options: optionsBarLine
    });

    const ctxUserRating = document.getElementById('user-rating-chart').getContext('2d');
    const rawData = dashboardData.feedbackRatings;
    const totalReviews = rawData.length;

    const feedbackCounts = [0, 0, 0, 0, 0];
    rawData.forEach(rating => {
        if (rating >= 1 && rating <= 5) {
            feedbackCounts[5 - rating] += 1;
        }
    });
    const percentageData = feedbackCounts.map(count => (count / totalReviews) * 100);

    const dataUserRating = {
        labels: ['5 Stars', '4 Stars', '3 Stars', '2 Stars', '1 Star'],
        datasets: [
            {
                label: 'Percentage',
                data: percentageData,
                backgroundColor: '#052542',
                borderColor: '#052542',
            }
        ]
    };

    const optionsUserRating = {
        indexAxis: 'y',
        scales: {
            x: {
                beginAtZero: true,
                max: 100,
                ticks: {
                    callback: function (value) {
                        return value + '%';
                    }
                }
            }
        },
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        let label = context.label || '';
                        let rawValue = feedbackCounts[context.dataIndex];
                        let percentageValue = context.raw.toFixed(2);
                        return `${label}: ${rawValue} reviews (${percentageValue}%)`;
                    }
                }
            }
        }
    };

    new Chart(ctxUserRating, {
        type: 'bar',
        data: dataUserRating,
        options: optionsUserRating
    });

    const averageRating = (dashboardData.averageFeedbackRating) * 1.0;

    const ratingValue = Math.trunc(averageRating);
    const stars = document.querySelectorAll('.star-rating .star');

    stars.forEach((star, index) => {
        if (index < ratingValue) {
            star.classList.add('selected');
        }
    });

    document.getElementById('average-rating').innerText = averageRating.toFixed(1);
    document.getElementById('total-reviews').innerText = totalReviews;

    const data = {
        labels: [
            `Software (${dashboardData.totalTicketsSoftware})`,
            `Hardware (${dashboardData.totalTicketsHardware})`,
            `Network (${dashboardData.totalTicketsNetwork})`,
            `Account (${dashboardData.totalTicketsAccount})`,
            `Other (${dashboardData.totalTicketsOther})`
        ],
        datasets: [{
            data: [
                dashboardData.totalTicketsSoftware,
                dashboardData.totalTicketsHardware,
                dashboardData.totalTicketsNetwork,
                dashboardData.totalTicketsAccount,
                dashboardData.totalTicketsOther
            ],
            backgroundColor: ['#1E88E5', '#8E24AA', '#3949AB', '#F4511E', '#6D4C41'],
            hoverBackgroundColor: ['#1E88E5', '#8E24AA', '#3949AB', '#F4511E', '#6D4C41']
        }]
    };

    const config = {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        boxWidth: 6,
                        padding: 20,
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            let label = context.label || '';
                            let value = context.raw || 0;
                            return `${label}: ${value}`;
                        }
                    }
                }
            }
        }
    };

    const pieChart = new Chart(
        document.getElementById('pie-chart'),
        config
    );
});
