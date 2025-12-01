/**
 * AcMap - Interactive Map Module using Leaflet.js
 * For Ashare MAUI Hybrid Blazor App
 */
window.AcMap = window.AcMap || {};

(function (AcMap) {
    // Store map instances by container ID
    const maps = {};

    /**
     * Initialize a map instance
     */
    AcMap.initialize = function (containerId, dotNetRef, options) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Map container not found:', containerId);
            return;
        }

        // Set container height
        container.style.height = options.height + 'px';
        container.innerHTML = ''; // Clear placeholder content

        // Check if Leaflet is loaded
        if (typeof L === 'undefined') {
            console.error('Leaflet.js is not loaded');
            container.innerHTML = '<div class="ac-map-error">خطأ في تحميل الخريطة</div>';
            return;
        }

        // Create map
        const map = L.map(containerId, {
            center: [options.latitude, options.longitude],
            zoom: options.zoom,
            zoomControl: true,
            attributionControl: false
        });

        // Add OpenStreetMap tiles
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19
        }).addTo(map);

        // Create custom marker icon
        const markerIcon = L.divIcon({
            className: 'ac-leaflet-marker',
            html: '<i class="bi bi-geo-alt-fill"></i>',
            iconSize: [40, 40],
            iconAnchor: [20, 40]
        });

        // Store instance
        maps[containerId] = {
            map: map,
            marker: null,
            dotNetRef: dotNetRef
        };

        // Add initial marker if coordinates provided
        if (options.hasMarker) {
            addMarker(containerId, options.latitude, options.longitude);
        }

        // Handle map click if interactive
        if (options.interactive) {
            map.on('click', function (e) {
                const lat = e.latlng.lat;
                const lng = e.latlng.lng;

                addMarker(containerId, lat, lng);

                // Notify Blazor
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnMapLocationSelected', lat, lng);
                }
            });
        }

        // Fix map size after render
        setTimeout(function () {
            map.invalidateSize();
        }, 100);
    };

    /**
     * Add or move marker
     */
    function addMarker(containerId, lat, lng) {
        const instance = maps[containerId];
        if (!instance) return;

        const markerIcon = L.divIcon({
            className: 'ac-leaflet-marker',
            html: '<i class="bi bi-geo-alt-fill"></i>',
            iconSize: [40, 40],
            iconAnchor: [20, 40]
        });

        if (instance.marker) {
            instance.marker.setLatLng([lat, lng]);
        } else {
            instance.marker = L.marker([lat, lng], {
                icon: markerIcon,
                draggable: true
            }).addTo(instance.map);

            // Handle marker drag
            instance.marker.on('dragend', function (e) {
                const pos = e.target.getLatLng();
                if (instance.dotNetRef) {
                    instance.dotNetRef.invokeMethodAsync('OnMapLocationSelected', pos.lat, pos.lng);
                }
            });
        }

        // Center map on marker
        instance.map.setView([lat, lng], instance.map.getZoom());
    }

    /**
     * Set marker position
     */
    AcMap.setMarker = function (containerId, lat, lng) {
        addMarker(containerId, lat, lng);
    };

    /**
     * Center map on location
     */
    AcMap.centerOn = function (containerId, lat, lng, zoom) {
        const instance = maps[containerId];
        if (instance) {
            instance.map.setView([lat, lng], zoom);
        }
    };

    /**
     * Destroy map instance
     */
    AcMap.destroy = function (containerId) {
        const instance = maps[containerId];
        if (instance) {
            instance.map.remove();
            delete maps[containerId];
        }
    };

    /**
     * Open native maps app with coordinates
     */
    AcMap.openNativeMaps = function (lat, lng, label) {
        label = label || 'الموقع';

        // Try to detect platform and open appropriate maps app
        const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);
        const isAndroid = /Android/.test(navigator.userAgent);

        let url;
        if (isIOS) {
            // Apple Maps
            url = `maps://maps.apple.com/?q=${encodeURIComponent(label)}&ll=${lat},${lng}`;
        } else if (isAndroid) {
            // Google Maps on Android
            url = `geo:${lat},${lng}?q=${lat},${lng}(${encodeURIComponent(label)})`;
        } else {
            // Fallback to Google Maps web
            url = `https://www.google.com/maps?q=${lat},${lng}`;
        }

        window.open(url, '_blank');
    };

})(window.AcMap);
