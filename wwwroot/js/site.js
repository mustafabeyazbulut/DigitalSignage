// Navigation Helper - Smart back navigation system
const NavigationHelper = {
    // Initialize navigation tracking
    init: function () {
        // Store current page in session for navigation tracking
        this.trackPageVisit();
    },

    // Track page visits for intelligent back navigation
    trackPageVisit: function () {
        const currentUrl = window.location.href;
        const currentPath = window.location.pathname;

        // Get navigation history from sessionStorage
        let navHistory = JSON.parse(sessionStorage.getItem('navHistory') || '[]');

        // Add current page to history (max 10 pages)
        navHistory.push({
            url: currentUrl,
            path: currentPath,
            timestamp: Date.now()
        });

        // Keep only last 10 pages
        if (navHistory.length > 10) {
            navHistory = navHistory.slice(-10);
        }

        sessionStorage.setItem('navHistory', JSON.stringify(navHistory));
    },

    // Smart back navigation - goes to previous page or fallback
    goBack: function (fallbackUrl) {
        // Check URL for returnUrl parameter (deep linking)
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl');

        // If returnUrl exists in URL, use it (highest priority)
        if (returnUrl) {
            if (typeof NavigationAnalytics !== 'undefined') {
                NavigationAnalytics.trackSmartBack('returnUrl');
            }
            window.location.href = returnUrl;
            return;
        }

        // Use breadcrumb-aware fallback URL (second priority)
        // This ensures navigation follows the breadcrumb hierarchy
        if (fallbackUrl && fallbackUrl !== window.location.pathname) {
            if (typeof NavigationAnalytics !== 'undefined') {
                NavigationAnalytics.trackSmartBack('breadcrumb');
            }
            window.location.href = fallbackUrl;
            return;
        }

        // Try to use browser history if available (last resort)
        if (window.history.length > 1 && document.referrer) {
            // Check if referrer is from same domain (security)
            try {
                const referrerDomain = new URL(document.referrer).hostname;
                const currentDomain = window.location.hostname;

                if (referrerDomain === currentDomain) {
                    if (typeof NavigationAnalytics !== 'undefined') {
                        NavigationAnalytics.trackSmartBack('history');
                    }
                    window.history.back();
                    return;
                }
            } catch (e) {
                // Invalid URL, continue to fallback
            }
        }

        // Default fallback to home
        if (typeof NavigationAnalytics !== 'undefined') {
            NavigationAnalytics.trackSmartBack('fallback');
        }
        window.location.href = fallbackUrl || '/';
    },

    // Get previous page from navigation history
    getPreviousPage: function () {
        const navHistory = JSON.parse(sessionStorage.getItem('navHistory') || '[]');

        // Get second to last page (last is current)
        if (navHistory.length >= 2) {
            return navHistory[navHistory.length - 2].url;
        }

        return null;
    },

    // Check if we can go back safely
    canGoBack: function () {
        return window.history.length > 1 && document.referrer !== '';
    },

    // Get returnUrl from current URL
    getReturnUrl: function () {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get('returnUrl');
    },

    // Build URL with returnUrl parameter for deep linking
    buildUrlWithReturn: function (targetUrl, returnUrl) {
        const url = new URL(targetUrl, window.location.origin);
        if (returnUrl) {
            url.searchParams.set('returnUrl', returnUrl);
        }
        return url.toString();
    },

    // Update all links to include returnUrl for deep linking
    enableDeepLinking: function () {
        // Add returnUrl to navigation links
        document.querySelectorAll('a[data-enable-deep-link]').forEach(function (link) {
            link.addEventListener('click', function (e) {
                const currentUrl = window.location.pathname;
                const targetUrl = this.getAttribute('href');

                // Build URL with returnUrl
                const newUrl = NavigationHelper.buildUrlWithReturn(targetUrl, currentUrl);
                this.setAttribute('href', newUrl);
            });
        });
    }
};

// Mobile Swipe Gesture Handler
const SwipeHandler = {
    touchStartX: 0,
    touchStartY: 0,
    touchEndX: 0,
    touchEndY: 0,
    minSwipeDistance: 50,  // Minimum 50px swipe
    swipeThreshold: 100,   // 100px to trigger back
    edgeZone: 50,          // Only work in 50px from left edge

    init: function () {
        if ('ontouchstart' in window) {
            document.addEventListener('touchstart', this.handleTouchStart.bind(this), { passive: true });
            document.addEventListener('touchmove', this.handleTouchMove.bind(this), { passive: false });
            document.addEventListener('touchend', this.handleTouchEnd.bind(this), { passive: true });
        }
    },

    handleTouchStart: function (e) {
        this.touchStartX = e.changedTouches[0].screenX;
        this.touchStartY = e.changedTouches[0].screenY;

        // Only activate if touch starts in edge zone
        if (this.touchStartX > this.edgeZone) {
            this.touchStartX = 0; // Disable swipe
        }
    },

    handleTouchMove: function (e) {
        if (this.touchStartX === 0) return;

        this.touchEndX = e.changedTouches[0].screenX;
        this.touchEndY = e.changedTouches[0].screenY;

        const deltaX = this.touchEndX - this.touchStartX;
        const deltaY = Math.abs(this.touchEndY - this.touchStartY);

        // If swiping right and not scrolling vertically
        if (deltaX > this.minSwipeDistance && deltaY < 50) {
            // Show visual feedback
            this.showSwipeFeedback(deltaX);

            // Prevent default scroll if swipe is detected
            if (deltaX > this.minSwipeDistance) {
                e.preventDefault();
            }
        }
    },

    handleTouchEnd: function (e) {
        if (this.touchStartX === 0) return;

        this.touchEndX = e.changedTouches[0].screenX;
        const deltaX = this.touchEndX - this.touchStartX;

        // Hide feedback
        this.hideSwipeFeedback();

        // If swipe distance exceeds threshold, go back
        if (deltaX > this.swipeThreshold) {
            // Track swipe gesture
            if (typeof NavigationAnalytics !== 'undefined') {
                NavigationAnalytics.trackSwipeGesture();
            }
            NavigationHelper.goBack();
        }

        // Reset
        this.touchStartX = 0;
        this.touchStartY = 0;
    },

    showSwipeFeedback: function (distance) {
        // Create or update feedback indicator
        let indicator = document.getElementById('swipe-feedback');
        if (!indicator) {
            indicator = document.createElement('div');
            indicator.id = 'swipe-feedback';
            indicator.style.cssText = `
                position: fixed;
                left: 0;
                top: 50%;
                transform: translateY(-50%);
                background: rgba(0, 123, 255, 0.1);
                border-right: 3px solid rgba(0, 123, 255, 0.5);
                height: 100%;
                transition: width 0.1s ease;
                pointer-events: none;
                z-index: 9999;
            `;
            document.body.appendChild(indicator);
        }

        // Update width based on swipe distance
        const width = Math.min(distance, this.swipeThreshold);
        indicator.style.width = width + 'px';

        // Change color if threshold reached
        if (distance > this.swipeThreshold) {
            indicator.style.background = 'rgba(40, 167, 69, 0.2)';
            indicator.style.borderColor = 'rgba(40, 167, 69, 0.8)';
        }
    },

    hideSwipeFeedback: function () {
        const indicator = document.getElementById('swipe-feedback');
        if (indicator) {
            indicator.remove();
        }
    }
};

// Initialize navigation helper when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    NavigationHelper.init();

    // Setup all back buttons with data-smart-back attribute
    document.querySelectorAll('[data-smart-back]').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const fallback = this.getAttribute('data-smart-back') || '/';
            NavigationHelper.goBack(fallback);
        });
    });

    // Enable deep linking for navigation
    NavigationHelper.enableDeepLinking();

    // Initialize mobile swipe gestures
    SwipeHandler.init();

    // Initialize navigation analytics
    NavigationAnalytics.init();

    // Expose analytics summary to console (for debugging)
    // Type: NavigationAnalytics.logSummary() in console
    console.log('💡 Tip: Type "NavigationAnalytics.logSummary()" to see navigation analytics');
});

// Navigation Analytics Tracker
const NavigationAnalytics = {
    init: function () {
        this.trackPageView();
        this.trackBackButtonUsage();
    },

    // Track page view
    trackPageView: function () {
        const analytics = this.getAnalytics();
        const currentPath = window.location.pathname;

        // Increment page view count
        if (!analytics.pageViews[currentPath]) {
            analytics.pageViews[currentPath] = {
                count: 0,
                firstVisit: Date.now(),
                lastVisit: Date.now()
            };
        }

        analytics.pageViews[currentPath].count++;
        analytics.pageViews[currentPath].lastVisit = Date.now();

        // Track navigation flow
        const previousPath = sessionStorage.getItem('lastPath');
        if (previousPath && previousPath !== currentPath) {
            const flow = `${previousPath} → ${currentPath}`;
            analytics.navigationFlows[flow] = (analytics.navigationFlows[flow] || 0) + 1;
        }

        sessionStorage.setItem('lastPath', currentPath);
        this.saveAnalytics(analytics);
    },

    // Track back button usage
    trackBackButtonUsage: function () {
        const analytics = this.getAnalytics();

        // Track which method was used
        window.addEventListener('popstate', function () {
            analytics.backButtonUsage.browserBack++;
            NavigationAnalytics.saveAnalytics(analytics);
        });
    },

    // Track smart back usage
    trackSmartBack: function (method) {
        const analytics = this.getAnalytics();

        if (method === 'history') {
            analytics.backButtonUsage.historyBackUsed++;
        } else if (method === 'fallback') {
            analytics.backButtonUsage.fallbackUsed++;
        } else if (method === 'returnUrl') {
            analytics.backButtonUsage.returnUrlUsed++;
        }

        this.saveAnalytics(analytics);
    },

    // Track swipe gesture
    trackSwipeGesture: function () {
        const analytics = this.getAnalytics();
        analytics.swipeGestures.total++;
        analytics.swipeGestures.lastUsed = Date.now();
        this.saveAnalytics(analytics);
    },

    // Get analytics data
    getAnalytics: function () {
        const stored = localStorage.getItem('navigationAnalytics');
        if (stored) {
            return JSON.parse(stored);
        }

        return {
            pageViews: {},
            navigationFlows: {},
            backButtonUsage: {
                browserBack: 0,
                historyBackUsed: 0,
                fallbackUsed: 0,
                returnUrlUsed: 0
            },
            swipeGestures: {
                total: 0,
                lastUsed: null
            },
            sessionStart: Date.now()
        };
    },

    // Save analytics data
    saveAnalytics: function (analytics) {
        localStorage.setItem('navigationAnalytics', JSON.stringify(analytics));
    },

    // Get analytics summary
    getSummary: function () {
        const analytics = this.getAnalytics();

        // Top pages
        const topPages = Object.entries(analytics.pageViews)
            .sort((a, b) => b[1].count - a[1].count)
            .slice(0, 10)
            .map(([path, data]) => ({
                path,
                views: data.count
            }));

        // Top flows
        const topFlows = Object.entries(analytics.navigationFlows)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 10)
            .map(([flow, count]) => ({
                flow,
                count
            }));

        // Back button stats
        const totalBackActions =
            analytics.backButtonUsage.browserBack +
            analytics.backButtonUsage.historyBackUsed +
            analytics.backButtonUsage.fallbackUsed +
            analytics.backButtonUsage.returnUrlUsed;

        return {
            topPages,
            topFlows,
            backButtonUsage: analytics.backButtonUsage,
            totalBackActions,
            swipeGestures: analytics.swipeGestures,
            sessionDuration: Date.now() - analytics.sessionStart
        };
    },

    // Clear analytics data
    clear: function () {
        localStorage.removeItem('navigationAnalytics');
        sessionStorage.removeItem('lastPath');
    },

    // Log summary to console (for debugging)
    logSummary: function () {
        const summary = this.getSummary();
        console.group('📊 Navigation Analytics Summary');
        console.log('Top Pages:', summary.topPages);
        console.log('Top Navigation Flows:', summary.topFlows);
        console.log('Back Button Usage:', summary.backButtonUsage);
        console.log('Total Back Actions:', summary.totalBackActions);
        console.log('Swipe Gestures:', summary.swipeGestures);
        console.log('Session Duration:', Math.round(summary.sessionDuration / 1000) + 's');
        console.groupEnd();
    }
};

// Make helpers globally available
window.NavigationHelper = NavigationHelper;
window.SwipeHandler = SwipeHandler;
window.NavigationAnalytics = NavigationAnalytics;
