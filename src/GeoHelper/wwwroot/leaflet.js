const map = L.map('map').setView({ lon: -84.386330, lat: 33.753746  }, 5);

let goesByName = {};

export function load_map() {
    // create the tile layer 
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(map);
}

export function add_geojson(raw, name, hex) {
    if (!hex.includes("#")) {
        hex = `#${hex}`
    }
    
    // create the geometry
    let geo = L.geoJSON(JSON.parse(raw), {
        style: function () {
            return { color: hex }
        }
    });
    
    geo.bindTooltip(name);
    geo.on('mouseover', function(e){
        geo.openTooltip();
    });
    geo.on('mouseout', function(e){
        geo.closeTooltip();
    });
    
    // add it to the map
    geo.addTo(map);

    goesByName[name] = geo;
}

export function remove_geo(name) {
    goesByName[name].remove()
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
