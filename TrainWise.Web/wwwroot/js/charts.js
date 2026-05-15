// ═══════════════════════════════════════════
//  TrainWise — Advanced Model Visualization
//  Powered by Plotly.js
// ═══════════════════════════════════════════

window.TrainWiseCharts = {
    /**
     * Render Confusion Matrix Heatmap
     */
    renderConfusionMatrix: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [{
            z: data.matrix,
            x: data.xLabels,
            y: data.yLabels,
            type: 'heatmap',
            colorscale: [
                [0, '#161b27'],
                [1, '#818cf8']
            ],
            showscale: false,
            text: data.matrix.map(row => row.map(val => val.toString())),
            texttemplate: '%{text}',
            textfont: { color: '#ffffff', size: 14, family: 'Plus Jakarta Sans' },
            hoverinfo: 'x+y+z'
        }];

        const layout = this._getBaseLayout('Predicted', 'Actual');
        layout.yaxis.autorange = 'reversed';
        layout.margin = { t: 10, r: 10, b: 40, l: 60 };

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render ROC Curve
     */
    renderRocCurve: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.baseline,
                y: data.baseline,
                mode: 'lines',
                name: 'Random',
                line: { color: '#64748b', dash: 'dash', width: 2 }
            },
            {
                x: data.fpr,
                y: data.tpr,
                mode: 'lines',
                name: `Model (AUC: ${data.auc})`,
                line: { color: '#818cf8', width: 3 }
            }
        ];

        const layout = this._getBaseLayout('False Positive Rate', 'True Positive Rate');
        layout.showlegend = true;
        layout.legend = { x: 1, xanchor: 'right', y: 0, font: { color: '#64748b', size: 10 } };

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Precision-Recall Curve
     */
    renderPrecisionRecall: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [{
            x: data.recall,
            y: data.precision,
            mode: 'lines',
            name: `Avg Precision: ${data.avgPrecision}`,
            line: { color: '#22d3ee', width: 3 },
            fill: 'tozeroy',
            fillcolor: 'rgba(34, 211, 238, 0.05)'
        }];

        const layout = this._getBaseLayout('Recall', 'Precision');
        layout.showlegend = true;
        layout.legend = { x: 1, xanchor: 'right', y: 0, font: { color: '#64748b', size: 10 } };

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Feature Importance Horizontal Bar
     */
    renderFeatureImportance: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        // Plotly bar charts render from bottom to top, so we reverse for descending view
        const names = [...data.names].reverse();
        const values = [...data.values].reverse();

        const plotData = [{
            y: names,
            x: values,
            type: 'bar',
            orientation: 'h',
            marker: {
                color: values,
                colorscale: [
                    [0, '#22d3ee'],
                    [1, '#818cf8']
                ]
            },
            hoverinfo: 'x'
        }];

        const layout = this._getBaseLayout('Importance Score', '');
        layout.margin = { t: 10, r: 20, b: 40, l: 120 };
        layout.yaxis.automargin = true;

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Actual vs Predicted Scatter
     */
    renderActualVsPredicted: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.reference,
                y: data.reference,
                mode: 'lines',
                name: 'Perfect Fit',
                line: { color: '#22d3ee', dash: 'dash', width: 2 }
            },
            {
                x: data.actual,
                y: data.predicted,
                mode: 'markers',
                name: 'Predictions',
                marker: { color: '#818cf8', opacity: 0.4, size: 8 }
            }
        ];

        const layout = this._getBaseLayout('Actual Values', 'Predicted Values');
        layout.showlegend = false;

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Residuals Scatter
     */
    renderResidualsScatter: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.zeroLine,
                y: [0, 0],
                mode: 'lines',
                name: 'Zero Error',
                line: { color: '#64748b', dash: 'dash', width: 2 }
            },
            {
                x: data.predicted,
                y: data.residuals,
                mode: 'markers',
                name: 'Residuals',
                marker: { color: '#f87171', opacity: 0.4, size: 8 }
            }
        ];

        const layout = this._getBaseLayout('Predicted Values', 'Residuals');
        layout.showlegend = false;

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Residuals Distribution Histogram
     */
    renderResidualsDistribution: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [{
            x: data.residuals,
            type: 'histogram',
            name: 'Residuals',
            marker: { color: '#34d399', opacity: 0.6 },
            nbinsx: 30
        }];

        const layout = this._getBaseLayout('Residual Value', 'Frequency');
        layout.annotations = [{
            xref: 'paper', yref: 'paper',
            x: 0.95, y: 0.95,
            text: `Mean: ${data.mean}<br>Std: ${data.std}`,
            showarrow: false,
            font: { color: '#64748b', size: 11 },
            align: 'right',
            bgcolor: 'rgba(22, 27, 39, 0.8)',
            bordercolor: 'rgba(148, 163, 184, 0.12)',
            borderwidth: 1,
            borderpad: 4
        }];

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Probability Distribution Histogram
     */
    renderProbabilityDist: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [{
            x: data.confidences,
            type: 'histogram',
            name: 'Confidence',
            marker: { color: '#818cf8', opacity: 0.6 },
            nbinsx: data.bins || 20
        }];

        const layout = this._getBaseLayout('Model Confidence (Max Prob)', 'Frequency');
        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Q-Q Plot
     */
    renderQqPlot: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.line,
                y: data.line,
                mode: 'lines',
                name: 'Normal Dist',
                line: { color: '#64748b', dash: 'dash', width: 2 }
            },
            {
                x: data.theoretical,
                y: data.sample,
                mode: 'markers',
                name: 'Residuals',
                marker: { color: '#fbbf24', opacity: 0.6, size: 7 }
            }
        ];

        const layout = this._getBaseLayout('Theoretical Quantiles', 'Sample Quantiles');
        layout.showlegend = false;
        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Class Comparison Bar
     */
    renderClassComparison: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.labels,
                y: data.actualCounts,
                type: 'bar',
                name: 'Actual',
                marker: { color: '#64748b', opacity: 0.5 }
            },
            {
                x: data.labels,
                y: data.predictedCounts,
                type: 'bar',
                name: 'Predicted',
                marker: { color: '#818cf8', opacity: 0.8 }
            }
        ];

        const layout = this._getBaseLayout('Class', 'Frequency');
        layout.barmode = 'group';
        layout.showlegend = true;
        layout.legend = { orientation: 'h', y: 1.1, x: 0.5, xanchor: 'center', font: { color: '#64748b' } };

        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Render Cross-Validation Variance
     */
    renderCrossValidation: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el || !data) return;

        const plotData = [
            {
                x: data.folds,
                y: data.scores,
                type: 'bar',
                name: 'Fold Score',
                marker: { color: '#34d399' }
            },
            {
                x: [data.folds[0], data.folds[data.folds.length - 1]],
                y: [data.mean, data.mean],
                mode: 'lines',
                name: 'Mean',
                line: { color: '#ffffff', dash: 'dot', width: 2 }
            }
        ];

        const layout = this._getBaseLayout('Cross-Validation Folds', 'Performance Score');
        layout.showlegend = false;
        Plotly.newPlot(el, plotData, layout, this._getConfig());
    },

    /**
     * Internal: Shared Base Layout
     */
    _getBaseLayout: function (xTitle, yTitle) {
        return {
            xaxis: { 
                title: { text: xTitle, font: { size: 12 } },
                color: '#64748b', 
                gridcolor: 'rgba(100, 116, 139, 0.1)',
                zeroline: false
            },
            yaxis: { 
                title: { text: yTitle, font: { size: 12 } },
                color: '#64748b', 
                gridcolor: 'rgba(100, 116, 139, 0.1)',
                zeroline: false
            },
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'transparent',
            font: { family: 'Plus Jakarta Sans, system-ui, sans-serif', color: '#cbd5e1' },
            margin: { t: 10, r: 10, b: 40, l: 50 },
            hovermode: 'closest'
        };
    },

    /**
     * Internal: Shared Config
     */
    _getConfig: function () {
        return {
            responsive: true,
            displayModeBar: false
        };
    }
};
