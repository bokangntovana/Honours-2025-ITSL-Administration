// wwwroot/js/quiz.js
class QuizManager {
    constructor(quizId, currentPage, totalQuestions) {
        this.quizId = quizId;
        this.currentPage = currentPage;
        this.totalQuestions = totalQuestions;
        this.storageKey = `quizAnswers_${quizId}`;

        this.init();
    }

    init() {
        this.loadSavedAnswers();
        this.setupAnswerHandlers();
        this.setupFinalSubmission();
        this.setupNavigationHandler();
        this.setupFinalSubmitHandler();
        this.setupPageUnloadHandler();

        console.log(`Quiz session initialized for quiz ${this.quizId}, page ${this.currentPage}`);
    }

    loadSavedAnswers() {
        const savedAnswers = this.getSavedAnswers();

        // Apply saved answers to current page questions
        document.querySelectorAll('input[type="radio"]').forEach(radio => {
            const questionId = this.getQuestionIdFromRadio(radio);
            if (questionId && savedAnswers[questionId] === radio.value) {
                radio.checked = true;
            }
        });
    }

    setupAnswerHandlers() {
        document.querySelectorAll('input[type="radio"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                const questionId = this.getQuestionIdFromRadio(e.target);
                if (questionId) {
                    this.saveAnswer(questionId, e.target.value);
                }
            });
        });
    }

    setupFinalSubmission() {
        const submitModal = document.getElementById('submitModal');
        if (submitModal) {
            submitModal.addEventListener('show.bs.modal', () => {
                this.saveCurrentPageAnswers();
                this.checkUnansweredQuestions();
                this.prepareFinalSubmission();
            });
        }
    }

    setupNavigationHandler() {
        const quizForm = document.getElementById('quizForm');
        if (quizForm) {
            quizForm.addEventListener('submit', () => {
                this.saveCurrentPageAnswers();
            });
        }
    }

    setupFinalSubmitHandler() {
        const finalSubmitForm = document.getElementById('finalSubmitForm');
        if (finalSubmitForm) {
            finalSubmitForm.addEventListener('submit', () => {
                // Clear the stored answers after submission
                sessionStorage.removeItem(this.storageKey);
            });
        }
    }

    setupPageUnloadHandler() {
        window.addEventListener('beforeunload', () => {
            this.saveCurrentPageAnswers();
        });
    }

    saveCurrentPageAnswers() {
        document.querySelectorAll('.question-container').forEach((container, index) => {
            const questionIdInput = container.querySelector('input[type="hidden"]');
            const selectedRadio = container.querySelector('input[type="radio"]:checked');

            if (questionIdInput && selectedRadio) {
                this.saveAnswer(questionIdInput.value, selectedRadio.value);
            }
        });
    }

    saveAnswer(questionId, answer) {
        const answers = this.getSavedAnswers();
        answers[questionId] = answer;
        sessionStorage.setItem(this.storageKey, JSON.stringify(answers));
    }

    getSavedAnswers() {
        return JSON.parse(sessionStorage.getItem(this.storageKey) || '{}');
    }

    getQuestionIdFromRadio(radio) {
        const name = radio.name;
        const match = name.match(/Questions\[(\d+)\]\.UserAnswer/);
        if (!match) return null;

        const index = parseInt(match[1], 10);
        const questionIdInput = document.querySelector(`input[name="Questions[${index}].QuestionId"]`);
        return questionIdInput ? questionIdInput.value : null;
    }

    checkUnansweredQuestions() {
        const savedAnswers = this.getSavedAnswers();
        const answeredCount = Object.keys(savedAnswers).length;
        const unansweredCount = this.totalQuestions - answeredCount;

        const warningElement = document.getElementById('unansweredWarning');
        const countElement = document.getElementById('unansweredCount');

        if (unansweredCount > 0) {
            countElement.textContent = unansweredCount;
            warningElement.classList.remove('d-none');
        } else {
            warningElement.classList.add('d-none');
        }
    }

    prepareFinalSubmission() {
        const container = document.getElementById('finalQuestionsContainer');
        const savedAnswers = this.getSavedAnswers();

        // Clear previous hidden inputs
        container.innerHTML = '';

        // Create hidden inputs for all questions and their answers
        let questionIndex = 0;
        Object.keys(savedAnswers).forEach(questionId => {
            const questionIdInput = document.createElement('input');
            questionIdInput.type = 'hidden';
            questionIdInput.name = `Questions[${questionIndex}].QuestionId`;
            questionIdInput.value = questionId;

            const answerInput = document.createElement('input');
            answerInput.type = 'hidden';
            answerInput.name = `Questions[${questionIndex}].UserAnswer`;
            answerInput.value = savedAnswers[questionId];

            container.appendChild(questionIdInput);
            container.appendChild(answerInput);
            questionIndex++;
        });

        console.log(`Prepared final submission with ${questionIndex} answered questions`);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Get quiz data from data attributes
    const quizCard = document.querySelector('.quiz-container');
    if (quizCard) {
        const quizId = quizCard.dataset.quizId;
        const currentPage = parseInt(quizCard.dataset.currentPage) || 1;
        const totalQuestions = parseInt(quizCard.dataset.totalQuestions) || 0;

        // Initialize quiz manager
        new QuizManager(quizId, currentPage, totalQuestions);
    }
});