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
        // Try to use browser history if available
        if (window.history.length > 1 && document.referrer) {
            // Check if referrer is from same domain (security)
            const referrerDomain = new URL(document.referrer).hostname;
            const currentDomain = window.location.hostname;

            if (referrerDomain === currentDomain) {
                window.history.back();
                return;
            }
        }

        // If no valid history, use fallback
        if (fallbackUrl) {
            window.location.href = fallbackUrl;
        } else {
            // Default fallback to home
            window.location.href = '/';
        }
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
});

// Make NavigationHelper globally available
window.NavigationHelper = NavigationHelper;
