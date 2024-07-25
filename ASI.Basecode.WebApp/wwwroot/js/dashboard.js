document.addEventListener('DOMContentLoaded', function () {
    const ctxBarLine = document.getElementById('bar-chart').getContext('2d');
    const dataBarLine = {
        labels: ['03 Mar', '04 Mar', '05 Mar', '06 Mar', '07 Mar', '08 Mar', '09 Mar'],
        datasets: [
            {
                type: 'bar',
                label: 'Bar Dataset',
                data: [50, 25, 125, 250, 200, 225, 100],
                backgroundColor: 'rgba(5, 37, 66, 1)',
                order: 1
            },
            {
                type: 'line',
                label: 'Line Dataset',
                data: [50, 25, 125, 250, 200, 225, 100],
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
                max: 300
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
    const rawData = [10000, 50, 2137, 210, 98];
    const totalReviews = rawData.reduce((a, b) => a + b, 0);

    const percentageData = rawData.map(count => (count / totalReviews) * 100);

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
                ticks: {
                    callback: function (value) {
                        return value + '%';
                    }
                }
            }
        },
        plugins: {
            tooltip: {
                callbacks: {
                    label: function (context) {
                        let label = context.label || '';
                        let rawValue = rawData[context.dataIndex];
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

    const averageRating = rawData.reduce((sum, count, index) => {
        return sum + count * (5 - index);
    }, 0) / totalReviews;

    const ratingValue = Math.trunc(averageRating);
    const stars = document.querySelectorAll('.star-rating .star');

    stars.forEach((star, index) => {
        if (index < ratingValue) {
            star.classList.add('selected');
        }
    });

    document.getElementById('average-rating').innerText = averageRating.toFixed(1);
    document.getElementById('total-reviews').innerText = totalReviews;
});

document.addEventListener('DOMContentLoaded', (event) => {
    const data = {
        labels: ['Software (99)', 'Hardware (8)', 'Network (509)', 'Account (406)', 'Other (16)'],
        datasets: [{
            data: [99, 8, 509, 406, 16],
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
