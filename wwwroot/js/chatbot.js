document.addEventListener("DOMContentLoaded", function () {
    // DOM Elements
    const toggle = document.getElementById("chatbot-toggle");
    const windowEl = document.getElementById("chatbot-window");
    const closeBtn = document.getElementById("chatbot-close");
    const toggleChatBtn = document.getElementById("toggle-chat");
    const toggleNavBtn = document.getElementById("toggle-nav");
    const backToNavBtn = document.getElementById("back-to-nav");
    const clearChatBtn = document.getElementById("clear-chat");
    const quickNavSection = document.getElementById("quick-nav-section");
    const chatSection = document.getElementById("chat-section");
    const chatMessages = document.getElementById("chat-messages");
    const chatInput = document.getElementById("chat-input");
    const sendMessageBtn = document.getElementById("send-message");

    // Knowledge base for Q&A
    const knowledgeBase = [
        {
            patterns: ['hello', 'hi', 'hey', 'greetings'],
            response: 'Hello! 👋 I\'m the ITSL assistant. How can I help you navigate our website today?',
            suggestions: ['How do I donate?', 'Where are my courses?', 'How to check grades?']
        },
        {
            patterns: ['donate', 'donation', 'give money', 'contribute', 'support'],
            response: 'You can make donations through our donation system. Here are your options:',
            actions: [
                { text: '💰 Make a Donation', url: '/Donations/Donations', icon: 'fa-donate' },
                { text: '❤️ Become a Regular Donor', url: '/Donations/RegisterDonor', icon: 'fa-hand-holding-heart' }
            ]
        },
        {
            patterns: ['course', 'classes', 'learning', 'study', 'material'],
            response: 'You can access all learning materials and courses here:',
            actions: [
                { text: '📚 View All Courses', url: '/Courses/Index', icon: 'fa-book' },
                { text: '📅 Classes & Events', url: '/EventSchedules/Index', icon: 'fa-calendar-alt' }
            ]
        },
        {
            patterns: ['assignment', 'homework', 'task', 'submit'],
            response: 'You can manage and submit your assignments here:',
            actions: [
                { text: '📝 Go to Assignments', url: '/Assignment/ManageAssignment?courseId=1', icon: 'fa-tasks' }
            ]
        },
        {
            patterns: ['grade', 'score', 'result', 'marks', 'performance'],
            response: 'You can check your academic performance and grades here:',
            actions: [
                { text: '📊 View My Grades', url: '/Grades/ParticipantGrades', icon: 'fa-chart-line' }
            ]
        },
        {
            patterns: ['quiz', 'test', 'exam', 'assessment'],
            response: 'You can take quizzes and assessments here:',
            actions: [
                { text: '❓ Take a Quiz', url: '/Quiz/AvailableQuiz', icon: 'fa-question-circle' }
            ]
        },
        {
            patterns: ['message', 'email', 'contact', 'communicate', 'inbox'],
            response: 'You can send and receive messages here:',
            actions: [
                { text: '✉️ Send Message', url: '/Email/SendEmail', icon: 'fa-envelope' }
            ]
        },
        {
            patterns: ['payment', 'pay', 'fee', 'transaction', 'bill'],
            response: 'You can manage payments and transactions here:',
            actions: [
                { text: '💳 Make Payment', url: '/Donations/Payment', icon: 'fa-credit-card' }
            ]
        },
        {
            patterns: ['login', 'sign in', 'log in', 'account'],
            response: 'You can access your account here:',
            actions: [
                { text: '🔐 Login', url: '/Account/Login', icon: 'fa-sign-in-alt' }
            ]
        },
        {
            patterns: ['register', 'sign up', 'create account', 'join'],
            response: 'You can create a new account here:',
            actions: [
                { text: '👤 Register', url: '/Account/Register', icon: 'fa-user-plus' }
            ]
        },
        {
            patterns: ['about', 'what is itsl', 'learn more', 'information'],
            response: 'Learn more about ITSL and our mission:',
            actions: [
                { text: 'ℹ️ About ITSL', url: '/Account/About', icon: 'fa-info-circle' }
            ]
        },
        {
            patterns: ['help', 'support', 'problem', 'issue', 'trouble'],
            response: 'I\'m here to help! 🛠️ You can ask me about: donations, courses, assignments, grades, quizzes, payments, or messaging. What do you need help with?',
            suggestions: ['How to donate?', 'Where are my courses?', 'How to check grades?', 'Payment issues?']
        }
    ];

    // Initialize chatbot
    function initChatbot() {
        // Toggle chatbot window
        toggle.addEventListener("click", () => {
            windowEl.classList.toggle("hidden");
        });

        closeBtn.addEventListener("click", () => {
            windowEl.classList.add("hidden");
        });

        // Toggle between navigation and chat
        toggleChatBtn.addEventListener("click", switchToChat);
        toggleNavBtn.addEventListener("click", switchToNavigation);
        backToNavBtn.addEventListener("click", switchToNavigation);

        // Clear chat
        clearChatBtn.addEventListener("click", clearChat);

        // Send message functionality
        sendMessageBtn.addEventListener("click", sendMessage);
        chatInput.addEventListener("keypress", (e) => {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });

        // Suggestion chip click handlers
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('suggestion-chip')) {
                const question = e.target.getAttribute('data-question');
                chatInput.value = question;
                sendMessage();
            }
        });
    }

    function switchToChat() {
        quickNavSection.classList.add('hidden');
        chatSection.classList.remove('hidden');
        toggleChatBtn.classList.add('hidden');
        toggleNavBtn.classList.remove('hidden');
        chatInput.focus();
    }

    function switchToNavigation() {
        chatSection.classList.add('hidden');
        quickNavSection.classList.remove('hidden');
        toggleNavBtn.classList.add('hidden');
        toggleChatBtn.classList.remove('hidden');
    }

    function clearChat() {
        const welcomeMessage = chatMessages.querySelector('.welcome-message');
        chatMessages.innerHTML = '';
        if (welcomeMessage) {
            chatMessages.appendChild(welcomeMessage.cloneNode(true));
        }
        addMessage('bot', 'Chat cleared! How can I help you navigate the ITSL website?', ['How do I donate?', 'Where are my courses?', 'How to check grades?']);
    }

    function sendMessage() {
        const message = chatInput.value.trim();
        if (message) {
            addMessage('user', message);
            chatInput.value = '';
            showTypingIndicator();
            processMessage(message);
        }
    }

    function addMessage(sender, message, suggestions = null) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `${sender}-message`;

        const avatar = document.createElement('div');
        avatar.className = 'message-avatar';
        avatar.innerHTML = `<i class="fas fa-${sender === 'user' ? 'user' : 'robot'}"></i>`;

        const content = document.createElement('div');
        content.className = 'message-content';

        const text = document.createElement('p');
        text.textContent = message;
        content.appendChild(text);

        if (suggestions && suggestions.length > 0) {
            const suggestionsDiv = document.createElement('div');
            suggestionsDiv.className = 'suggestions';
            suggestions.forEach(suggestion => {
                const chip = document.createElement('span');
                chip.className = 'suggestion-chip';
                chip.setAttribute('data-question', suggestion);

                // Extract icon from suggestion text
                let icon = 'fa-question';
                if (suggestion.includes('donate')) icon = 'fa-donate';
                else if (suggestion.includes('course')) icon = 'fa-book';
                else if (suggestion.includes('grade')) icon = 'fa-chart-line';
                else if (suggestion.includes('payment')) icon = 'fa-credit-card';

                chip.innerHTML = `<i class="fas ${icon}"></i> ${suggestion}`;
                suggestionsDiv.appendChild(chip);
            });
            content.appendChild(suggestionsDiv);
        }

        messageDiv.appendChild(avatar);
        messageDiv.appendChild(content);
        chatMessages.appendChild(messageDiv);

        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function showTypingIndicator() {
        const typingDiv = document.createElement('div');
        typingDiv.className = 'bot-message';
        typingDiv.id = 'typing-indicator';

        const avatar = document.createElement('div');
        avatar.className = 'message-avatar';
        avatar.innerHTML = '<i class="fas fa-robot"></i>';

        const content = document.createElement('div');
        content.className = 'message-content';

        const typing = document.createElement('div');
        typing.className = 'typing-indicator';
        typing.innerHTML = `
            <div class="typing-dot"></div>
            <div class="typing-dot"></div>
            <div class="typing-dot"></div>
        `;

        content.appendChild(typing);
        typingDiv.appendChild(avatar);
        typingDiv.appendChild(content);
        chatMessages.appendChild(typingDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function hideTypingIndicator() {
        const typingIndicator = document.getElementById('typing-indicator');
        if (typingIndicator) {
            typingIndicator.remove();
        }
    }

    function addActionButtons(actions) {
        const actionsDiv = document.createElement('div');
        actionsDiv.className = 'action-buttons';

        actions.forEach(action => {
            const button = document.createElement('a');
            button.href = action.url;
            button.className = 'action-button';
            button.innerHTML = `<i class="fas ${action.icon}"></i> ${action.text}`;
            actionsDiv.appendChild(button);
        });

        chatMessages.appendChild(actionsDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function processMessage(message) {
        const lowerMessage = message.toLowerCase();
        let response = null;
        let actions = null;
        let suggestions = null;

        // Find matching pattern in knowledge base
        for (const item of knowledgeBase) {
            if (item.patterns.some(pattern => lowerMessage.includes(pattern))) {
                response = item.response;
                actions = item.actions || null;
                suggestions = item.suggestions || null;
                break;
            }
        }

        // Default response if no match found
        if (!response) {
            response = "I'm not sure I understand. 🤔 Try asking about: donations, courses, assignments, grades, quizzes, or messaging. How else can I help?";
            suggestions = ['How to donate?', 'Where are my courses?', 'How to check grades?', 'Payment issues?'];
        }

        // Simulate processing delay
        setTimeout(() => {
            hideTypingIndicator();
            addMessage('bot', response, suggestions);
            if (actions) {
                setTimeout(() => addActionButtons(actions), 100);
            }
        }, 1000 + Math.random() * 1000);
    }

    // Initialize the chatbot
    initChatbot();
});