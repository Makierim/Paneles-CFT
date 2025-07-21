// Archivo: wwwroot/js/site.js

let selectedLat = null;
let selectedLng = null;
let map;
let marker;
let monthlyChart;

document.addEventListener('DOMContentLoaded', function () {
    map = L.map('map').setView([-37.7972, -72.6983], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    map.on('click', function (e) {
        selectedLat = e.latlng.lat.toFixed(4);
        selectedLng = e.latlng.lng.toFixed(4);
        document.getElementById('latitude').value = selectedLat;
        document.getElementById('longitude').value = selectedLng;
        if (marker) map.removeLayer(marker);
        marker = L.marker([selectedLat, selectedLng]).addTo(map).bindPopup(`Ubicación: ${selectedLat}, ${selectedLng}`).openPopup();
        ['getSolarDataBtn', 'calculatePvProductionBtn', 'getHourlyDataBtn'].forEach(id => document.getElementById(id).disabled = false);
    });

    ['getSolarDataBtn', 'calculatePvProductionBtn', 'getHourlyDataBtn'].forEach(id => document.getElementById(id).disabled = true);
    document.getElementById('getOptimalAngles').addEventListener('change', function () {
        document.getElementById('manualAngles').style.display = this.checked ? 'none' : 'flex';
    });

    document.getElementById('getSolarDataBtn').addEventListener('click', getSolarData);
    document.getElementById('calculatePvProductionBtn').addEventListener('click', calculatePvProduction);
    document.getElementById('getHourlyDataBtn').addEventListener('click', getHourlySolarData);
});

function toggleLoading(spinnerId, errorId, show) {
    document.getElementById(spinnerId).style.display = show ? 'block' : 'none';
    if (errorId) document.getElementById(errorId).textContent = '';
}

function displayError(errorId, message) {
    document.getElementById(errorId).textContent = `Error: ${message}`;
}

// --- NUEVA FUNCIÓN AUXILIAR (SOLO PARA BOTÓN 3) ---
function getNumericValue(elementId) {
    const value = document.getElementById(elementId).value;
    return parseFloat(value.replace(',', '.'));
}

// --- FUNCIÓN BOTÓN 1 (INTACTA) ---
async function getSolarData() {
    if (!selectedLat || !selectedLng) return;
    toggleLoading('solarDataSpinner', 'solarDataError', true);
    document.getElementById('mrCalcResults').style.display = 'none';

    try {
        const response = await fetch(`/api/SolarProxy/SolarData?lat=${selectedLat}&lng=${selectedLng}`);
        if (!response.ok) throw new Error(await response.text());
        const data = await response.json();

        // --- DEPURACIÓN BOTÓN 1 ---
        console.log("--- INICIO DEPURACIÓN BOTÓN 1 ---");
        console.log("1. Datos completos recibidos del servidor:", data);
        console.log("2. ¿Datos disponibles?", data.solarDataAvailable);
        console.log("3. Valor de irradiación anual a mostrar:", data.annualIrradiance_kWh_m2);
        console.log("4. Arreglo de datos mensuales a procesar:", data.monthlyIrradiance);
        console.log("--- FIN DEPURACIÓN BOTÓN 1 ---");
        // ---

        if (data.solarDataAvailable) {
            document.getElementById('annualIrradiance').textContent = data.annualIrradiance_kWh_m2.toFixed(2);
            const monthlyTableBody = document.getElementById('monthlyIrradianceTableBody');
            monthlyTableBody.innerHTML = '';
            data.monthlyIrradiance.forEach(monthData => {
                const row = monthlyTableBody.insertRow();
                row.insertCell(0).textContent = monthData.month;
                row.insertCell(1).textContent = monthData.irradiance_kWh_m2.toFixed(2);
            });
            document.getElementById('mrCalcResults').style.display = 'block';
        }
    } catch (error) {
        console.error("Error detallado en Botón 1:", error);
        displayError('solarDataError', error.message);
    } finally {
        toggleLoading('solarDataSpinner', 'solarDataError', false);
    }
}

// --- FUNCIÓN BOTÓN 2 (INTACTA) ---
async function calculatePvProduction() {
    if (!selectedLat || !selectedLng) return;
    toggleLoading('pvCalcSpinner', 'pvCalcError', true);
    document.getElementById('pvCalcResults').style.display = 'none';

    const params = {
        latitude: parseFloat(selectedLat),
        longitude: parseFloat(selectedLng),
        peakPower: parseFloat(document.getElementById('peakPower').value),
        systemLosses: parseFloat(document.getElementById('systemLosses').value),
        inclinationAngle: document.getElementById('getOptimalAngles').checked ? null : parseFloat(document.getElementById('inclinationAngle').value),
        azimuthAngle: document.getElementById('getOptimalAngles').checked ? null : parseFloat(document.getElementById('azimuthAngle').value),
        getOptimalAngles: document.getElementById('getOptimalAngles').checked
    };

    try {
        const response = await fetch('/api/SolarProxy/PvSystemProduction', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(params)
        });
        if (!response.ok) throw new Error(await response.text());
        const data = await response.json();

        // --- DEPURACIÓN BOTÓN 2 (PERMANENTE) ---
        console.log("--- INICIO DEPURACIÓN BOTÓN 2 ---");
        console.log("1. Datos completos recibidos del servidor:", data);
        console.log("2. Accediendo a data.outputs.totals.fixed:", data.outputs.totals.fixed);
        console.log("3. Accediendo a data.outputs.monthly.fixed:", data.outputs.monthly.fixed);
        console.log("--- FIN DEPURACIÓN BOTÓN 2 ---");
        // ---

        const totals = data.outputs.totals.fixed;
        const monthlyDataList = data.outputs.monthly.fixed;

        document.getElementById('pvCalcAnnualProduction').textContent = totals.E_y.toFixed(2);

        const monthlyTableBody = document.getElementById('pvCalcMonthlyProductionTableBody');
        monthlyTableBody.innerHTML = '';
        monthlyDataList.forEach(monthData => {
            const row = monthlyTableBody.insertRow();
            row.insertCell(0).textContent = new Date(2000, monthData.month, 1).toLocaleString('es-ES', { month: 'long' });
            row.insertCell(1).textContent = monthData.E_m.toFixed(2);
        });

        document.getElementById('lossTemp').textContent = totals.l_tg.toFixed(2);
        document.getElementById('lossAOI').textContent = totals.l_aoi.toFixed(2);
        document.getElementById('lossShading').textContent = "N/A";
        document.getElementById('lossSystem').textContent = params.systemLosses.toFixed(2);
        document.getElementById('lossTotalPvgis').textContent = totals.l_total.toFixed(2);

        document.getElementById('pvCalcResults').style.display = 'block';
    } catch (error) {
        console.error("Error detallado en Botón 2:", error);
        displayError('pvCalcError', error.message);
    } finally {
        toggleLoading('pvCalcSpinner', 'pvCalcError', false);
    }
}

// =============================================================
// === INICIO DE LA SECCIÓN MODIFICADA PARA EL TERCER BOTÓN ===
// =============================================================

// Función auxiliar para leer y convertir números con coma decimal
function getNumericValue(elementId) {
    const value = document.getElementById(elementId).value;
    // Reemplaza la coma por un punto para asegurar que JS lo interprete como decimal
    return parseFloat(value.replace(',', '.'));
}

async function getHourlySolarData() {
    if (!selectedLat || !selectedLng) return;
    toggleLoading('hourlyDataSpinner', 'hourlyDataError', true);
    document.getElementById('solarCalcDetailedResults').style.display = 'none';

    // MODIFICADO: Usar la función getNumericValue para leer los parámetros correctamente
    const params = {
        lat: selectedLat,
        lng: selectedLng,
        angle: getNumericValue('inclinationAngle'),
        aspect: getNumericValue('azimuthAngle'),
        panelEfficiency: getNumericValue('panelEfficiency') / 100,
        panelArea: getNumericValue('panelArea'),
        numberOfPanels: getNumericValue('numberOfPanels'),
        inverterEfficiency: getNumericValue('inverterEfficiency') / 100,
        temperatureCoefficient: getNumericValue('tempCoefficient') / 100,
        noct: getNumericValue('noct'),
        useoptimalangles: document.getElementById('getOptimalAngles').checked
    };
    const apiUrl = `/api/SolarProxy/HourlySolarData?${new URLSearchParams(params)}`;

    try {
        const response = await fetch(apiUrl);
        if (!response.ok) throw new Error(await response.text());
        const data = await response.json();

        // --- INICIO DEPURACIÓN FRONTEND ---
        console.log("--- DEPURACIÓN BOTÓN 3 (FRONTEND) ---");
        console.log("1. Datos procesados recibidos del backend:", data);
        console.log("2. Energía anual calculada:", data.annualEnergy_kWh);
        console.log("3. Arreglo de energía mensual:", data.monthlyEnergy_kWh);
        // --- FIN DEPURACIÓN ---

        if (data.annualEnergy_kWh > 0) {
            document.getElementById('solarCalcAnnualProduction').textContent = data.annualEnergy_kWh.toFixed(2);

            const monthlyTableBody = document.getElementById('solarCalcMonthlyProductionTableBody');
            monthlyTableBody.innerHTML = '';
            const monthlyLabels = [];
            const monthlySolarCalcData = [];
            const monthNames = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

            data.monthlyEnergy_kWh.forEach(monthData => {
                const row = monthlyTableBody.insertRow();
                row.insertCell(0).textContent = new Date(2000, monthData.month, 1).toLocaleString('es-ES', { month: 'long' });
                row.insertCell(1).textContent = monthData.energy_kWh.toFixed(2);
                monthlyLabels.push(monthNames[monthData.month - 1]);
                monthlySolarCalcData.push(monthData.energy_kWh.toFixed(2));
            });

            if (data.optimalInclinationAngle) {
                document.getElementById('optimalInclinationSolarCalc').textContent = data.optimalInclinationAngle.toFixed(1);
                document.getElementById('optimalAzimuthSolarCalc').textContent = data.optimalAzimuthAngle.toFixed(1);
                document.getElementById('solarCalcOptimalAngles').style.display = 'block';
            } else {
                document.getElementById('solarCalcOptimalAngles').style.display = 'none';
            }

            document.getElementById('solarCalcDetailedResults').style.display = 'block';

            if (monthlyChart) monthlyChart.destroy();
            const ctx = document.getElementById('monthlyProductionChart').getContext('2d');
            monthlyChart = new Chart(ctx, {
                type: 'bar',
                data: { labels: monthlyLabels, datasets: [{ label: 'Producción Mensual (kWh)', data: monthlySolarCalcData, backgroundColor: 'rgba(54, 162, 235, 0.6)' }] },
                options: { responsive: true, scales: { y: { beginAtZero: true } } }
            });
        } else {
            displayError('hourlyDataError', 'No se pudieron generar los datos para el gráfico.');
        }
    } catch (error) {
        console.error("Error detallado en Botón 3:", error);
        displayError('hourlyDataError', error.message);
    } finally {
        toggleLoading('hourlyDataSpinner', 'hourlyDataError', false);
    }
}