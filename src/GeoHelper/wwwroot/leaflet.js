export async function importFile(streamRef) {
    const data = await streamRef.arrayBuffer();
    const geojson = await shp(data);
    
    let count = 0;
    geojson.features.forEach((feat) => {
        
        let name = "";
        if (feat.properties){
            Object.keys(feat.properties).forEach((key) => {
                name += `<b>${key}:</b> ${feat.properties[key]}<br>`;
            });
        }
        
        if (!name) {
            name = `<b>Polygon:</b> ${count++}`;
        }
        
        name += "<br><br>Right-click to remove."
        
        let hex = '#'+(Math.random() * 0xFFFFFF << 0).toString(16).padStart(6, '0');
        add_geojson_internal(feat, name, hex)
    });
}

export async function importDbf(streamRef) {
    const data = await streamRef.arrayBuffer();
    let dbf = shp.parseDbf(data);
    return Object.keys(dbf[0]);
}

export async function get_viewport() {
    return map.getBounds();
}

const map = L.map('map').setView({ lon: -84.386330, lat: 33.753746  }, 5);

let goesByName = {};

export function load_map() {
    // create the tile layer 
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(map);
}

export function add_geojson_internal(geojson, name, hex) {
    // create the geometry
    let geo = L.geoJSON(geojson, {
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
    
    geo.on("contextmenu", function(e) {
       geo.remove()
    });

    // add it to the map
    geo.addTo(map);

    goesByName[name] = geo;
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


