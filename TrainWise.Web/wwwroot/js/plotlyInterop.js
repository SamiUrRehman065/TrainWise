// ═══════════════════════════════════════════
//  TrainWise — Plotly.js Interop Functions
//  All chart rendering via JS called from Blazor
// ═══════════════════════════════════════════

window.PlotlyInterop = {
    /**
     * Render a confusion matrix heatmap.
     * @param {string} elementId - Target div ID
     * @param {number[][]} matrix - 2D array of values
     * @param {string[]} labels - Class labels
     */
    renderConfusionMatrix: function (elementId, matrix, labels) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const data = [{
            z: matrix,
            x: labels,
            y: labels,
            type: 'heatmap',
            colorscale: [
                [0, '#F7F5F0'],
                [0.5, '#C3DDD6'],
                [1, '#1B4D3E']
            ],
            showscale: true,
            hoverongaps: false,
            text: matrix.map(row => row.map(val => val.toString())),
            texttemplate: '%{text}',
            textfont: { color: '#1A1816', size: 14 }
        }];

        const layout = {
            xaxis: { title: 'Predicted', color: '#8A8278', gridcolor: '#DED9D0' },
            yaxis: { title: 'Actual', color: '#8A8278', gridcolor: '#DED9D0', autorange: 'reversed' },
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            margin: { t: 30, r: 30, b: 60, l: 60 }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a metrics bar chart (accuracy, precision, recall, F1).
     * @param {string} elementId - Target div ID
     * @param {string[]} metricNames - e.g. ["Accuracy", "Precision", "Recall", "F1"]
     * @param {number[]} metricValues - corresponding values
     */
    renderMetricsBar: function (elementId, metricNames, metricValues) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const colors = metricNames.map((_, i) => {
            const palette = ['#1B4D3E', '#185FA5', '#B33C2C', '#164035'];
            return palette[i % palette.length];
        });

        const data = [{
            x: metricNames,
            y: metricValues,
            type: 'bar',
            marker: {
                color: colors,
                line: { color: '#DED9D0', width: 1 }
            },
            text: metricValues.map(v => (v * 100).toFixed(1) + '%'),
            textposition: 'outside',
            textfont: { color: '#1A1816', size: 12 }
        }];

        const layout = {
            yaxis: {
                range: [0, 1.15],
                title: 'Score',
                color: '#8A8278',
                gridcolor: '#DED9D0',
                tickformat: '.0%'
            },
            xaxis: { color: '#8A8278' },
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            margin: { t: 30, r: 20, b: 50, l: 60 },
            bargap: 0.3
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a feature importance horizontal bar chart.
     * @param {string} elementId - Target div ID
     * @param {string[]} featureNames
     * @param {number[]} importances
     */
    renderFeatureImportance: function (elementId, featureNames, importances) {
        const el = document.getElementById(elementId);
        if (!el) return;

        // Sort by importance descending, take top 15
        const paired = featureNames.map((name, i) => ({ name, value: importances[i] }));
        paired.sort((a, b) => a.value - b.value);
        const top = paired.slice(-15);

        const data = [{
            y: top.map(p => p.name),
            x: top.map(p => p.value),
            type: 'bar',
            orientation: 'h',
            marker: {
                color: top.map(p => p.value),
                colorscale: [[0, '#DED9D0'], [1, '#1B4D3E']],
                line: { width: 0 }
            },
            text: top.map(p => p.value.toFixed(4)),
            textposition: 'outside',
            textfont: { color: '#8A8278', size: 11 }
        }];

        const layout = {
            xaxis: { title: 'Importance', color: '#8A8278', gridcolor: '#DED9D0' },
            yaxis: { color: '#8A8278', automargin: true },
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816', size: 11 },
            margin: { t: 20, r: 60, b: 50, l: 10 }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a Pearson correlation heatmap.
     * @param {string} elementId
     * @param {number[][]} matrix
     * @param {string[]} columnNames
     */
    renderCorrelationHeatmap: function (elementId, matrix, columnNames) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const data = [{
            z: matrix,
            x: columnNames,
            y: columnNames,
            type: 'heatmap',
            colorscale: [
                [0, '#B33C2C'],
                [0.5, '#F7F5F0'],
                [1, '#1B4D3E']
            ],
            zmin: -1,
            zmax: 1,
            showscale: true,
            text: matrix.map(row => row.map(val => val.toFixed(2))),
            texttemplate: '%{text}',
            textfont: { color: '#1A1816', size: 10 }
        }];

        const layout = {
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816', size: 10 },
            margin: { t: 30, r: 30, b: 80, l: 80 },
            xaxis: { tickangle: -45, color: '#8A8278' },
            yaxis: { color: '#8A8278', autorange: 'reversed' }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a class distribution pie chart.
     * @param {string} elementId
     * @param {string[]} labels - Class names
     * @param {number[]} values - Counts per class
     */
    renderClassDistribution: function (elementId, labels, values) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const colors = ['#1B4D3E', '#185FA5', '#B33C2C', '#8B6914', '#6B4A9B', '#C4500A', '#0B6E6E', '#993556'];

        const data = [{
            labels: labels,
            values: values,
            type: 'pie',
            hole: 0.45,
            marker: {
                colors: colors.slice(0, labels.length),
                line: { color: '#FFFFFF', width: 2 }
            },
            textinfo: 'label+percent',
            textfont: { color: '#1A1816', size: 12 },
            hoverinfo: 'label+value+percent'
        }];

        const layout = {
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            margin: { t: 30, r: 30, b: 30, l: 30 },
            showlegend: true,
            legend: { font: { color: '#8A8278', size: 11 } }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a regression metrics comparison bar (R², RMSE, MAE).
     * @param {string} elementId
     * @param {object} metrics - { r2Score, rmse, mae }
     */
    renderRegressionMetrics: function (elementId, metrics) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const names = ['R² Score', 'RMSE', 'MAE'];
        const values = [metrics.r2Score || 0, metrics.rmse || 0, metrics.mae || 0];
        const colors = ['#1B4D3E', '#185FA5', '#B33C2C'];

        const data = [{
            x: names,
            y: values,
            type: 'bar',
            marker: { color: colors, line: { color: '#DED9D0', width: 1 } },
            text: values.map(v => v.toFixed(4)),
            textposition: 'outside',
            textfont: { color: '#1A1816', size: 12 }
        }];

        const layout = {
            yaxis: { title: 'Value', color: '#8A8278', gridcolor: '#DED9D0' },
            xaxis: { color: '#8A8278' },
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            margin: { t: 30, r: 20, b: 50, l: 60 },
            bargap: 0.3
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Destroy a Plotly chart to free memory.
     * @param {string} elementId
     */
    destroy: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            Plotly.purge(el);
        }
    },

    renderSimpleBar: function (elementId, labels, values, title) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const data = [{
            x: labels,
            y: values,
            type: 'bar',
            marker: { color: '#1B4D3E' }
        }];
        const layout = {
            title: title || '',
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            xaxis: { color: '#8A8278', tickangle: -35 },
            yaxis: { color: '#8A8278', gridcolor: '#DED9D0' },
            margin: { t: 40, r: 20, b: 80, l: 50 }
        };
        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    renderHorizontalBar: function (elementId, labels, values, title) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const data = [{
            y: labels,
            x: values,
            type: 'bar',
            orientation: 'h',
            marker: { color: '#185FA5' }
        }];
        const layout = {
            title: title || '',
            paper_bgcolor: 'transparent',
            plot_bgcolor: '#FFFFFF',
            font: { family: 'Syne, DM Sans, sans-serif', color: '#1A1816' },
            xaxis: { color: '#8A8278', gridcolor: '#DED9D0' },
            yaxis: { color: '#8A8278', automargin: true },
            margin: { t: 40, r: 20, b: 40, l: 120 }
        };
        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    }
};
