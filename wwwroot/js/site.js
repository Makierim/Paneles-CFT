// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Variables globales para la latitud y longitud seleccionadas
let selectedLat = null;
let selectedLng = null;
let map; // Variable para el objeto mapa de Leaflet
let marker; // Variable para el marcador en el mapa
let monthlyChart; // Variable para la instancia del gráfico

document.addEventListener('DOMContentLoaded', function () {
    // Inicializar el mapa
    map = L.map('map').setView([-37.7972, -72.6983], 13); // Centrado inicial en Angol, Chile

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // Manejar clic en el mapa para obtener coordenadas
    map.on('click', function (e) {
        selectedLat = e.latlng.lat.toFixed(4);
        selectedLng = e.latlng.lng.toFixed(4);

        document.getElementById('latitude').value = selectedLat;
        document.getElementById('longitude').value = selectedLng;

        // Remover marcador anterior si existe
        if (marker) {
            map.removeLayer(marker);
        }

        // Añadir nuevo marcador
        marker = L.marker([selectedLat, selectedLng]).addTo(map)
            .bindPopup(`Ubicación seleccionada: ${selectedLat}, ${selectedLng}`)
            .openPopup();

        // Habilitar botones al seleccionar una ubicación
        document.getElementById('getSolarDataBtn').disabled = false;
        document.getElementById('calculatePvProductionBtn').disabled = false;
        document.getElementById('getHourlyDataBtn').disabled = false;

        // Ocultar resultados anteriores al seleccionar nueva ubicación
        document.getElementById('mrCalcResults').style.display = 'none';
        document.getElementById('pvCalcResults').style.display = 'none';
        document.getElementById('solarCalcResults').style.display = 'none';
        document.getElementById('solarDataError').textContent = '';
        document.getElementById('pvCalcError').textContent = '';
        document.getElementById('hourlyDataError').textContent = '';
    });

    // Deshabilitar botones al inicio si no hay ubicación seleccionada
    document.getElementById('getSolarDataBtn').disabled = true;
    document.getElementById('calculatePvProductionBtn').disabled = true;
    document.getElementById('getHourlyDataBtn').disabled = true;

    // Manejar el checkbox de ángulos óptimos
    const getOptimalAnglesCheckbox = document.getElementById('getOptimalAngles');
    const manualAnglesDiv = document.getElementById('manualAngles');

    getOptimalAnglesCheckbox.addEventListener('change', function () {
        if (this.checked) {
            manualAnglesDiv.style.display = 'none'; // Ocultar campos de ángulos manuales
        } else {
            manualAnglesDiv.style.display = 'flex'; // Mostrar campos de ángulos manuales
        }
    });

    // Disparar click en el botón de obtener datos solares
    document.getElementById('getSolarDataBtn').addEventListener('click', getSolarData);

    // Disparar click en el botón de calcular producción FV (PVcalc)
    document.getElementById('calculatePvProductionBtn').addEventListener('click', calculatePvProduction);

    // Disparar click en el botón de obtener datos horarios (seriescalc + SolarCalculator)
    document.getElementById('getHourlyDataBtn').addEventListener('click', getHourlySolarData);
});

// Función para mostrar/ocultar spinner y errores
function toggleLoading(spinnerId, errorId, show) {
    document.getElementById(spinnerId).style.display = show ? 'block' : 'none';
    document.getElementById(errorId).textContent = ''; // Limpiar errores al iniciar carga
}

function displayError(errorId, message) {
    document.getElementById(errorId).textContent = `Error: ${message}`;
}

// ----------------------------------------------------------------------
// Funciones para llamadas a la API de SolarProxyController
// ----------------------------------------------------------------------

// 1. Obtener datos de irradiación (MRcalc)
async function getSolarData() {
    if (!selectedLat || !selectedLng) {
        alert("Por favor, seleccione una ubicación en el mapa primero.");
        return;
    }

    toggleLoading('solarDataSpinner', 'solarDataError', true);
    document.getElementById('mrCalcResults').style.display = 'none'; // Ocultar resultados anteriores

    try {
        const response = await fetch(`/SolarProxy/SolarData?lat=${selectedLat}&lng=${selectedLng}`);
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
        }
        const data = await response.json();

        if (data.solarDataAvailable) {
            document.getElementById('annualIrradiance').textContent = data.annualIrradiance_kWh_m2.toFixed(2);
            const monthlyTableBody = document.getElementById('monthlyIrradianceTableBody');
            monthlyTableBody.innerHTML = ''; // Limpiar tabla

            data.monthlyIrradiance.forEach(monthData => {
                const row = monthlyTableBody.insertRow();
                row.insertCell(0).textContent = monthData.month;
                row.insertCell(1).textContent = monthData.irradiance_kWh_m2.toFixed(2);
            });
            document.getElementById('mrCalcResults').style.display = 'block';
        } else {
            displayError('solarDataError', 'No se pudieron obtener datos solares para la ubicación seleccionada.');
            document.getElementById('mrCalcResults').style.display = 'none';
        }
    } catch (error) {
        console.error("Error al obtener datos solares:", error);
        displayError('solarDataError', error.message || 'Error desconocido al obtener datos solares.');
    } finally {
        toggleLoading('solarDataSpinner', 'solarDataError', false);
    }
}

// 2. Calcular producción FV (PVcalc)
async function calculatePvProduction() {
    if (!selectedLat || !selectedLng) {
        alert("Por favor, seleccione una ubicación en el mapa primero.");
        return;
    }

    toggleLoading('pvCalcSpinner', 'pvCalcError', true);
    document.getElementById('pvCalcResults').style.display = 'none'; // Ocultar resultados anteriores

    const getOptimalAngles = document.getElementById('getOptimalAngles').checked;
    let inclinationAngle = null;
    let azimuthAngle = null;

    if (!getOptimalAngles) {
        inclinationAngle = parseFloat(document.getElementById('inclinationAngle').value);
        azimuthAngle = parseFloat(document.getElementById('azimuthAngle').value);
        if (isNaN(inclinationAngle) || isNaN(azimuthAngle)) {
            displayError('pvCalcError', 'Por favor ingrese ángulos válidos o seleccione "Calcular Ángulos Óptimos".');
            toggleLoading('pvCalcSpinner', 'pvCalcError', false);
            return;
        }
    }

    const peakPower = parseFloat(document.getElementById('peakPower').value);
    const systemLosses = parseFloat(document.getElementById('systemLosses').value);

    if (isNaN(peakPower) || isNaN(systemLosses)) {
        displayError('pvCalcError', 'Por favor ingrese valores válidos para Potencia Pico y Pérdidas del Sistema.');
        toggleLoading('pvCalcSpinner', 'pvCalcError', false);
        return;
    }

    const params = {
        latitude: parseFloat(selectedLat),
        longitude: parseFloat(selectedLng),
        peakPower: peakPower,
        systemLosses: systemLosses,
        inclinationAngle: inclinationAngle,
        azimuthAngle: azimuthAngle,
        getOptimalAngles: getOptimalAngles
    };

    try {
        const response = await fetch('/SolarProxy/PvSystemProduction', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(params)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
        }
        const data = await response.json();

        // Mostrar producción anual y mensual
        document.getElementById('pvCalcAnnualProduction').textContent = data.outputs.totals.E_PV.toFixed(2);
        const monthlyTableBody = document.getElementById('pvCalcMonthlyProductionTableBody');
        monthlyTableBody.innerHTML = '';
        data.outputs.monthly.forEach(monthData => {
            const row = monthlyTableBody.insertRow();
            row.insertCell(0).textContent = new Date(2000, monthData.month - 1, 1).toLocaleString('es-ES', { month: 'long' }); // Obtener nombre del mes
            row.insertCell(1).textContent = monthData.E_PV.toFixed(2);
        });

        // Mostrar pérdidas
        document.getElementById('lossTemp').textContent = data.outputs.losses.L_temp.toFixed(2);
        document.getElementById('lossAOI').textContent = data.outputs.losses.L_aoi.toFixed(2);
        document.getElementById('lossShading').textContent = data.outputs.losses.L_sh.toFixed(2);
        document.getElementById('lossSystem').textContent = data.outputs.losses.L_total.toFixed(2); // PVGIS L_total incluye varias pérdidas internas, incluyendo la system_losses ingresada.
        document.getElementById('lossTotalPvgis').textContent = data.outputs.losses.L_total.toFixed(2); // Usamos L_total para mostrar las pérdidas totales calculadas por PVGIS.

        // Mostrar ángulos óptimos si se solicitaron y están disponibles
        if (data.outputs.optimal_angles && data.outputs.optimal_angles.angle && data.outputs.optimal_angles.aspect) {
            document.getElementById('optimalInclinationPvCalc').textContent = data.outputs.optimal_angles.angle.value.toFixed(1);
            document.getElementById('optimalAzimuthPvCalc').textContent = data.outputs.optimal_angles.aspect.value.toFixed(1);
            document.getElementById('pvCalcOptimalAngles').style.display = 'block';
        } else {
            document.getElementById('pvCalcOptimalAngles').style.display = 'none';
        }

        document.getElementById('pvCalcResults').style.display = 'block';

    } catch (error) {
        console.error("Error al calcular producción FV:", error);
        displayError('pvCalcError', error.message || 'Error desconocido al calcular producción FV.');
    } finally {
        toggleLoading('pvCalcSpinner', 'pvCalcError', false);
    }
}

// 3. Obtener datos horarios y calcular producción con SolarCalculator (seriescalc)
async function getHourlySolarData() {
    if (!selectedLat || !selectedLng) {
        alert("Por favor, seleccione una ubicación en el mapa primero.");
        return;
    }

    toggleLoading('hourlyDataSpinner', 'hourlyDataError', true);
    document.getElementById('solarCalcResults').style.display = 'none'; // Ocultar resultados anteriores

    const getOptimalAngles = document.getElementById('getOptimalAngles').checked;
    let inclinationAngle = parseFloat(document.getElementById('inclinationAngle').value);
    let azimuthAngle = parseFloat(document.getElementById('azimuthAngle').value);

    const panelEfficiency = parseFloat(document.getElementById('panelEfficiency').value);
    const panelArea = parseFloat(document.getElementById('panelArea').value);
    const numberOfPanels = parseInt(document.getElementById('numberOfPanels').value);
    const inverterEfficiency = parseFloat(document.getElementById('inverterEfficiency').value);
    // Convertir de porcentaje a decimal para el coeficiente de temperatura
    const tempCoefficient = parseFloat(document.getElementById('tempCoefficient').value) / 100;
    const noct = parseFloat(document.getElementById('noct').value);

    if (isNaN(panelEfficiency) || isNaN(panelArea) || isNaN(numberOfPanels) ||
        isNaN(inverterEfficiency) || isNaN(tempCoefficient) || isNaN(noct)) {
        displayError('hourlyDataError', 'Por favor, complete todos los parámetros detallados del sistema FV.');
        toggleLoading('hourlyDataSpinner', 'hourlyDataError', false);
        return;
    }

    // Construir URL con parámetros, incluyendo los del SolarCalculator
    const apiUrl = `/SolarProxy/HourlySolarData?lat=${selectedLat}&lng=${selectedLng}` +
        `&angle=${inclinationAngle}&aspect=${azimuthAngle}` +
        `&panelEfficiency=${panelEfficiency / 100}` + // Convertir % a decimal
        `&panelArea=${panelArea}` +
        `&numberOfPanels=${numberOfPanels}` +
        `&inverterEfficiency=${inverterEfficiency / 100}` + // Convertir % a decimal
        `&temperatureCoefficient=${tempCoefficient}` +
        `&noct=${noct}` +
        `&useoptimalangles=${getOptimalAngles}`;

    try {
        const response = await fetch(apiUrl);
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
        }
        const data = await response.json(); // Data será del tipo HourlySolarDataResponse

        // Mostrar resultados de tu SolarCalculator
        document.getElementById('solarCalcAnnualProduction').textContent = data.annualEnergy_kWh.toFixed(2);

        const monthlyTableBody = document.getElementById('solarCalcMonthlyProductionTableBody');
        monthlyTableBody.innerHTML = '';
        const monthlyLabels = [];
        const monthlySolarCalcData = [];

        const monthNames = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];

        data.monthlyEnergy_kWh.forEach(monthData => {
            const row = monthlyTableBody.insertRow();
            const monthName = monthNames[monthData.month - 1]; // Ajustar índice para el array
            row.insertCell(0).textContent = monthName;
            row.insertCell(1).textContent = monthData.energy_kWh.toFixed(2);

            monthlyLabels.push(monthName);
            monthlySolarCalcData.push(monthData.energy_kWh.toFixed(2));
        });

        // Mostrar ángulos óptimos si se solicitaron y están disponibles
        if (data.optimalInclinationAngle && data.optimalAzimuthAngle) {
            document.getElementById('optimalInclinationSolarCalc').textContent = data.optimalInclinationAngle.toFixed(1);
            document.getElementById('optimalAzimuthSolarCalc').textContent = data.optimalAzimuthAngle.toFixed(1);
            document.getElementById('solarCalcOptimalAngles').style.display = 'block';
        } else {
            document.getElementById('solarCalcOptimalAngles').style.display = 'none';
        }

        document.getElementById('solarCalcResults').style.display = 'block';

        // Destruir instancia anterior del gráfico si existe
        if (monthlyChart) {
            monthlyChart.destroy();
        }

        // Crear o actualizar gráfico mensual de producción
        const ctx = document.getElementById('monthlyProductionChart').getContext('2d');
        monthlyChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: monthlyLabels,
                datasets: [{
                    label: 'Producción Mensual (kWh)',
                    data: monthlySolarCalcData,
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Energía (kWh)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Mes'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Producción Mensual Estimada'
                    }
                }
            }
        });

    } catch (error) {
        console.error("Error al obtener datos horarios y calcular producción:", error);
        displayError('hourlyDataError', error.message || 'Error desconocido al obtener datos horarios y calcular producción.');
    } finally {
        toggleLoading('hourlyDataSpinner', 'hourlyDataError', false);
    }
}