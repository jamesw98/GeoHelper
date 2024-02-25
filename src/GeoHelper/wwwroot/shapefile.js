// import { add_geojson } from "./leaflet";

export async function streamToJavaScript(streamRef) {
    const data = await streamRef.arrayBuffer();
    const geojson = await shp(data);
    geojson.features.forEach((feat) => {
       console.log(feat.properties.NAME) 
    });
}