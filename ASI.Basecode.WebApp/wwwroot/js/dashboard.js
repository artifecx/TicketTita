document.addEventListener('DOMContentLoaded', function () {
    // Bar Chart
    const ctxBar = document.getElementById('bar-chart').getContext('2d');
    const dataBarCustomTooltip = {
        labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
        datasets: [
            {
                label: 'Time (hour)',
                data: [30, 15, 62, 65, 61, 65, 40], // example raw numbers hrs
                backgroundColor: '#052542',
                borderColor: '#052542',
                borderWidth: 1
            }
        ]
    };

    const optionsBarCustomTooltip = {
        plugins: {
            tooltip: {
                callbacks: {
                    label: function (context) {
                        let label = context.dataset.label || '';
                        label = `${label}: ${context.raw} hr`;
                        return label;
                    }
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true
            }
        }
    };

    new Chart(ctxBar, {
        type: 'bar',
        data: dataBarCustomTooltip,
        options: optionsBarCustomTooltip
    });

    // User Rating Chart
    const ctxUserRating = document.getElementById('user-rating-chart').getContext('2d');
    const rawData = [10000, 50, 2137, 210, 98]; // example raw numbers
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

    // Update rating summary
    const averageRating = rawData.reduce((sum, count, index) => {
        return sum + count * (5 - index);
    }, 0) / totalReviews;

    // Update stars based on averageRating
    const ratingValue = Math.trunc(averageRating);
    const stars = document.querySelectorAll('.star-rating .star');

    stars.forEach((star, index) => {
        if (index < ratingValue) {
            star.classList.add('selected');
        }
    });

    // Update average rating display
    document.getElementById('average-rating').innerText = averageRating.toFixed(1);
    document.getElementById('total-reviews').innerText = totalReviews;
});