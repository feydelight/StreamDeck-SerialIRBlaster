function SaveAndClose() {
    console.log('Saving');
    var displayName = document.getElementById('displayName').value ?? '';
    var comPort = document.getElementById('comPort').value ?? null;
    var baudRate = document.getElementById('baudRate').value ?? null;
    var dataBits = document.getElementById('dataBits').value ?? null;
    var parity = document.getElementById('parity').value ?? null;
    var stopBit = document.getElementById('stopBit').value ?? null;

    if (!comPort) {
        window.alert('A serial port must be available to continue!');
        return;
    }
    var data = {
        displayName,
        comPort,
        baudRate,
        dataBits,
        parity,
        stopBit
    };
    window.opener.AddSerialPort(data);
    window.close();
}

function Remove(comPort) {
    if (!comPort) {
        window.alert('Serial Port ID not available!');
        return;
    }
    window.opener.RemoveSerialPort(comPort);
    window.close();
}

function PopulateExistingPorts() {
    var gs = window.opener.GetGlobalSettings();
    if (!gs) {
        return;
    }

    var globalSettings = gs.globalSettings;
    if (globalSettings && globalSettings.serialPortSettings) {
        DrawTable(globalSettings.serialPortSettings);
    }
    var serials = gs.serials;
    if (serials && serials.length) {
        AddSerialsToComPort(serials);
    }
}

function AddSerialsToComPort(serials) {
    if (!serials) {
        window.alert('No serial devices detected. Make sure your device is connected and it\'s serial port is not being used.');
        return;
    }
    var ddl = document.getElementById('comPort');
    if (!ddl) {
        console.log('Drop down was not found');
        return;
    }
    for (var i = 0; i < serials.length; ++i) {
        var opt = new Option(serials[i].desc, serials[i].id);
        ddl.options.add(opt);
    }
}

function DrawTable(serialPortSettings) {
    var div = document.getElementById('info');
    if (!serialPortSettings) {
        div.innerText = 'No serial devices have been setup yet.';
    }
    var tbl = document.createElement('table');
    var row = document.createElement('tr');

    // create header
    var th;
    var columnName = ['displayName', 'comPort', 'baudRate', 'dataBits', 'parity', 'stopBit', 'actions'];
    for (var i = 0; i < columnName.length; ++i) {
        th = document.createElement('th');
        th.textContent = columnName[i];
        row.appendChild(th);
    }
    tbl.appendChild(row);

    var td;



    Object.keys(serialPortSettings).forEach(function (key) {
        var val = serialPortSettings[key];
        if (val) {
            row = document.createElement('tr');
            row.classList.add('leftAlign');
            for (var i = 0; i < columnName.length; ++i) {
                td = document.createElement('td');
                var c = columnName[i];
                if (c === 'actions') {
                    var deleteBtn = document.createElement('button');
                    deleteBtn.textContent = 'Delete';
                    deleteBtn.addEventListener('click', function () { Remove(val['comPort']) });
                    td.appendChild(deleteBtn);
                } else {
                    td.textContent = val[c];
                }
                row.appendChild(td);
            }
            tbl.appendChild(row);
        }
    });
    div.appendChild(tbl);
}

PopulateExistingPorts();