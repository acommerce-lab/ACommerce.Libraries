/**
 * AcMap - Interactive Map Module using Leaflet.js
 * For Ashare MAUI Hybrid Blazor App
 * Optimized for BlazorWebView compatibility
 */
window.AcMap = window.AcMap || {};

(function (AcMap) {
    // Store map instances by container ID
    const maps = {};

    /**
     * Initialize a map instance
     */
    AcMap.initialize = function (containerId, dotNetRef, options) {
        // Safety check - destroy existing instance first
        if (maps[containerId]) {
            try {
                AcMap.destroy(containerId);
            } catch (e) {
                console.warn('Error destroying existing map:', e);
            }
        }

        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Map container not found:', containerId);
            return false;
        }

        // Ensure container has explicit dimensions (critical for MAUI WebView)
        container.style.height = options.height + 'px';
        container.style.width = '100%';
        container.innerHTML = ''; // Clear placeholder content

        // Check if Leaflet is loaded
        if (typeof L === 'undefined') {
            console.error('Leaflet.js is not loaded');
            container.innerHTML = '<div class="ac-map-error">خطأ في تحميل الخريطة</div>';
            return false;
        }

        try {
            // Create map with error handling
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

            // Store instance
            maps[containerId] = {
                map: map,
                marker: null,
                dotNetRef: dotNetRef,
                resizeObserver: null
            };

            // Add initial marker if coordinates provided
            if (options.hasMarker) {
                addMarker(containerId, options.latitude, options.longitude, options.interactive);
            }

            // Handle map click if interactive
            if (options.interactive) {
                map.on('click', function (e) {
                    const lat = e.latlng.lat;
                    const lng = e.latlng.lng;

                    addMarker(containerId, lat, lng, true);

                    // Notify Blazor (with error handling)
                    if (dotNetRef) {
                        try {
                            dotNetRef.invokeMethodAsync('OnMapLocationSelected', lat, lng)
                                .catch(function(err) {
                                    console.warn('Error notifying Blazor:', err);
                                });
                        } catch (e) {
                            console.warn('Error invoking Blazor method:', e);
                        }
                    }
                });
            }

            // Setup ResizeObserver for container size changes (helps with MAUI WebView)
            if (typeof ResizeObserver !== 'undefined') {
                const resizeObserver = new ResizeObserver(function() {
                    if (maps[containerId] && maps[containerId].map) {
                        try {
                            maps[containerId].map.invalidateSize();
                        } catch (e) {
                            // Ignore resize errors
                        }
                    }
                });
                resizeObserver.observe(container);
                maps[containerId].resizeObserver = resizeObserver;
            }

            // Fix map size after render (multiple attempts for MAUI WebView)
            setTimeout(function () {
                if (maps[containerId] && maps[containerId].map) {
                    maps[containerId].map.invalidateSize();
                }
            }, 100);

            setTimeout(function () {
                if (maps[containerId] && maps[containerId].map) {
                    maps[containerId].map.invalidateSize();
                }
            }, 300);

            return true;
        } catch (e) {
            console.error('Error initializing map:', e);
            container.innerHTML = '<div class="ac-map-error">خطأ في تحميل الخريطة</div>';
            return false;
        }
    };

    /**
     * Add or move marker
     */
    function addMarker(containerId, lat, lng, draggable) {
        const instance = maps[containerId];
        if (!instance || !instance.map) return;

        try {
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
                    draggable: draggable !== false
                }).addTo(instance.map);

                // Handle marker drag (only if draggable)
                if (draggable !== false) {
                    instance.marker.on('dragend', function (e) {
                        const pos = e.target.getLatLng();
                        if (instance.dotNetRef) {
                            try {
                                instance.dotNetRef.invokeMethodAsync('OnMapLocationSelected', pos.lat, pos.lng)
                                    .catch(function(err) {
                                        console.warn('Error notifying Blazor on drag:', err);
                                    });
                            } catch (e) {
                                console.warn('Error invoking Blazor method on drag:', e);
                            }
                        }
                    });
                }
            }

            // Center map on marker
            instance.map.setView([lat, lng], instance.map.getZoom());
        } catch (e) {
            console.warn('Error adding marker:', e);
        }
    }

    /**
     * Set marker position
     */
    AcMap.setMarker = function (containerId, lat, lng) {
        addMarker(containerId, lat, lng, true);
    };

    /**
     * Center map on location
     */
    AcMap.centerOn = function (containerId, lat, lng, zoom) {
        const instance = maps[containerId];
        if (instance && instance.map) {
            try {
                instance.map.setView([lat, lng], zoom);
            } catch (e) {
                console.warn('Error centering map:', e);
            }
        }
    };

    /**
     * Destroy map instance safely
     */
    AcMap.destroy = function (containerId) {
        const instance = maps[containerId];
        if (instance) {
            try {
                // Disconnect ResizeObserver
                if (instance.resizeObserver) {
                    instance.resizeObserver.disconnect();
                }

                // Remove marker first
                if (instance.marker) {
                    try {
                        instance.marker.remove();
                    } catch (e) { }
                }

                // Remove map
                if (instance.map) {
                    try {
                        instance.map.off(); // Remove all event listeners
                        instance.map.remove();
                    } catch (e) { }
                }
            } catch (e) {
                console.warn('Error during map destruction:', e);
            } finally {
                delete maps[containerId];
            }
        }
    };

    /**
     * Initialize a read-only mini map for details page
     */
    AcMap.initializeDetailsMap = function (containerId, lat, lng, label) {
        // Destroy existing instance
        if (maps[containerId]) {
            try {
                AcMap.destroy(containerId);
            } catch (e) {
                console.warn('Error destroying existing details map:', e);
            }
        }

        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Details map container not found:', containerId);
            return false;
        }

        // Set container dimensions
        container.style.height = '200px';
        container.style.width = '100%';
        container.innerHTML = '';

        if (typeof L === 'undefined') {
            console.error('Leaflet.js is not loaded');
            container.innerHTML = '<div class="ac-map-error">خطأ في تحميل الخريطة</div>';
            return false;
        }

        try {
            const map = L.map(containerId, {
                center: [lat, lng],
                zoom: 15,
                zoomControl: false,
                attributionControl: false,
                dragging: false,
                touchZoom: false,
                doubleClickZoom: false,
                scrollWheelZoom: false,
                boxZoom: false,
                keyboard: false
            });

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19
            }).addTo(map);

            // Add marker
            const markerIcon = L.divIcon({
                className: 'ac-leaflet-marker details',
                html: '<i class="bi bi-geo-alt-fill"></i>',
                iconSize: [40, 40],
                iconAnchor: [20, 40]
            });

            const marker = L.marker([lat, lng], {
                icon: markerIcon,
                draggable: false
            }).addTo(map);

            if (label) {
                marker.bindPopup(label).openPopup();
            }

            // Store instance
            maps[containerId] = {
                map: map,
                marker: marker,
                dotNetRef: null,
                resizeObserver: null
            };

            // Setup ResizeObserver
            if (typeof ResizeObserver !== 'undefined') {
                const resizeObserver = new ResizeObserver(function() {
                    if (maps[containerId] && maps[containerId].map) {
                        try {
                            maps[containerId].map.invalidateSize();
                        } catch (e) { }
                    }
                });
                resizeObserver.observe(container);
                maps[containerId].resizeObserver = resizeObserver;
            }

            // Fix size after render
            setTimeout(function () {
                if (maps[containerId] && maps[containerId].map) {
                    maps[containerId].map.invalidateSize();
                }
            }, 100);

            return true;
        } catch (e) {
            console.error('Error initializing details map:', e);
            container.innerHTML = '<div class="ac-map-error">خطأ في تحميل الخريطة</div>';
            return false;
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

        try {
            window.open(url, '_blank');
        } catch (e) {
            // Fallback to location change if popup blocked
            window.location.href = url;
        }
    };

    /**
     * Check if a map instance exists
     */
    AcMap.exists = function (containerId) {
        return maps[containerId] != null;
    };

    /**
     * Initialize explore map with multiple markers
     */
    AcMap.initializeExploreMap = function (containerId, dotNetRef, markers) {
        // Destroy existing instance
        if (maps[containerId]) {
            try {
                AcMap.destroy(containerId);
            } catch (e) {
                console.warn('Error destroying existing explore map:', e);
            }
        }

        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Explore map container not found:', containerId);
            return false;
        }

        // Ensure container has dimensions
        container.style.height = '100%';
        container.style.width = '100%';
        container.innerHTML = '';

        if (typeof L === 'undefined') {
            console.error('Leaflet.js is not loaded');
            return false;
        }

        try {
            // Default center (Riyadh)
            let center = [24.7136, 46.6753];
            let zoom = 10;

            // If we have markers, center on them
            if (markers && markers.length > 0) {
                const lats = markers.map(m => m.lat);
                const lngs = markers.map(m => m.lng);
                center = [
                    (Math.min(...lats) + Math.max(...lats)) / 2,
                    (Math.min(...lngs) + Math.max(...lngs)) / 2
                ];

                // Adjust zoom based on spread
                const latSpread = Math.max(...lats) - Math.min(...lats);
                const lngSpread = Math.max(...lngs) - Math.min(...lngs);
                const spread = Math.max(latSpread, lngSpread);
                if (spread < 0.01) zoom = 15;
                else if (spread < 0.1) zoom = 13;
                else if (spread < 0.5) zoom = 11;
                else zoom = 10;
            }

            const map = L.map(containerId, {
                center: center,
                zoom: zoom,
                zoomControl: true,
                attributionControl: false
            });

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19
            }).addTo(map);

            // Store instance
            maps[containerId] = {
                map: map,
                markers: [],
                dotNetRef: dotNetRef,
                resizeObserver: null
            };

            // Add markers
            if (markers && markers.length > 0) {
                markers.forEach(function (m) {
                    const markerIcon = L.divIcon({
                        className: 'ac-leaflet-price-marker',
                        html: '<div class="ac-price-tag">' + m.price + '</div>',
                        iconSize: [80, 32],
                        iconAnchor: [40, 32]
                    });

                    const marker = L.marker([m.lat, m.lng], {
                        icon: markerIcon
                    }).addTo(map);

                    marker.on('click', function () {
                        if (dotNetRef) {
                            try {
                                dotNetRef.invokeMethodAsync('OnMarkerClicked', m.id)
                                    .catch(function(err) {
                                        console.warn('Error notifying Blazor on marker click:', err);
                                    });
                            } catch (e) {
                                console.warn('Error invoking Blazor method:', e);
                            }
                        }
                    });

                    maps[containerId].markers.push(marker);
                });
            }

            // Setup ResizeObserver
            if (typeof ResizeObserver !== 'undefined') {
                const resizeObserver = new ResizeObserver(function() {
                    if (maps[containerId] && maps[containerId].map) {
                        try {
                            maps[containerId].map.invalidateSize();
                        } catch (e) { }
                    }
                });
                resizeObserver.observe(container);
                maps[containerId].resizeObserver = resizeObserver;
            }

            // Fix size after render
            setTimeout(function () {
                if (maps[containerId] && maps[containerId].map) {
                    maps[containerId].map.invalidateSize();
                }
            }, 100);

            setTimeout(function () {
                if (maps[containerId] && maps[containerId].map) {
                    maps[containerId].map.invalidateSize();
                }
            }, 300);

            return true;
        } catch (e) {
            console.error('Error initializing explore map:', e);
            return false;
        }
    };

})(window.AcMap);
