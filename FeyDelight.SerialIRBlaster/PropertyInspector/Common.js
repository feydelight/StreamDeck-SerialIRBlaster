function openFeyDelights() {
    if (websocket && (websocket.readyState === 1)) {
        const json = {
            'event': 'openUrl',
            'payload': {
                'url': 'https://github.com/feydelight/StreamDeck-SerialIRBlaster'
            }
        };
        websocket.send(JSON.stringify(json));
    }
}


function Log(message) {
    console.log('[IRBlaster]: ' + message);
}
if (window.websocket) {
    AddSocketMessage();
} else {
    document.addEventListener('websocketCreate', function () {
        AddSocketMessage();
    });
}

function AddSocketMessage() {
    websocket.addEventListener('message', function (event) {
        var jsonObj = JSON.parse(event.data);
        if (jsonObj.event == 'sendToPropertyInspector') {
            Log('Recieved event ' + jsonObj.event);
            var serials = jsonObj.payload.serials;
            Log('Serials: ' + JSON.stringify(serials, null, true));
            var ddl = document.getElementById('comPort');
            if (ddl == null) {
                Log('Drop down \'comPort\' not found.');
                return;
            }

            Log('Removing all unselected items...');
            // remove everything but the selected item
            for (var i = 0; i < ddl.options.length; ++i) {
                var opt = ddl.options[i];
                if (opt.selected == false) {
                    ddl.options.remove(i);
                    --i;
                }
            }

            Log('Adding all available serials...')
            // add the rest of the options.
            for (var i = 0; i < serials.length; ++i) {
                var key = serials[i].id;
                var desc = serials[i].desc;
                var opts = ddl.selectedOptions;
                if (opts.length > 0 && opts[0].value == key) {
                    // update the selected item's description
                    Log('Selected Item ' + key + ' was found. Updating its description: ' + desc);
                    opts[0].text = desc;
                } else {
                    Log('Adding new item: ' + key + ':' + desc);
                    var opt = new Option(desc, key, false, false);
                    ddl.options.add(opt);
                }
            }
        } else {
            Log('ignoring event: ' + jsonObj.event);
        }
    });
}