document.addEventListener("DOMContentLoaded", function () {
    const studentId = 1;

    // Global Toast Notification System
    window.showToast = function (message, type = "info") {
        const container = document.getElementById("toast-container");
        if (!container) return;

        const toast = document.createElement("div");
        toast.className = `toast-notification toast-${type}`;
        
        let icon = "bi-info-circle";
        if (type === "success") icon = "bi-check-circle-fill";
        if (type === "error") icon = "bi-exclamation-triangle-fill";

        toast.innerHTML = `
            <i class="bi ${icon}"></i>
            <span>${escapeHtml(message)}</span>
        `;

        container.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = "0";
            toast.style.transform = "translateY(10px)";
            toast.style.transition = "all 0.3s ease";
            setTimeout(() => toast.remove(), 300);
        }, 3500);
    };

    // Global Spinner/Button Loading Helper
    window.setButtonLoading = function (btn, isLoading, loadingText = "Loading...") {
        if (!btn) return;
        if (isLoading) {
            btn.dataset.originalText = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="display:inline-block; width:1rem; height:1rem; border:2px solid currentColor; border-right-color:transparent; border-radius:50%; animation:spin 0.75s linear infinite;"></span> ${loadingText}`;
        } else {
            btn.disabled = false;
            if (btn.dataset.originalText) {
                btn.innerHTML = btn.dataset.originalText;
            }
        }
    };

    // Chat Page Functionality
    const chatInput = document.getElementById("chat-input");
    const chatMessages = document.getElementById("chat-messages");

    window.sendChatMessage = async function (customPrompt) {
        const prompt = customPrompt || (chatInput ? chatInput.value.trim() : "");
        if (!prompt) return;

        if (chatInput) chatInput.value = "";

        appendUserMessage(prompt);
        const loadingDiv = appendLoadingMessage();

        try {
            const response = await fetch("/api/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ studentId: studentId, prompt: prompt })
            });

            const data = await response.json();
            loadingDiv.remove();

            if (!response.ok || data.error) {
                appendAiMessage("⚠️ " + (data.error || "Failed to process chat request."));
                showToast("AI Service Notice: " + (data.error || "Error occurred."), "error");
            } else {
                appendAiMessage(data.response || "Operation completed.");
                showToast("EduBuddy replied!", "success");
            }
        } catch (err) {
            console.error("Chat error:", err);
            loadingDiv.remove();
            appendAiMessage("Sorry, I encountered a connection error.");
            showToast("Network error communicating with AI service.", "error");
        }
    };

    function appendUserMessage(text) {
        if (!chatMessages) return;
        const msgDiv = document.createElement("div");
        msgDiv.style.display = "flex";
        msgDiv.style.justifyContent = "flex-end";
        msgDiv.innerHTML = `
            <div style="background:#edf2ff; color:#1e293b; padding:15px; border-radius:12px; max-width:600px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
                ${escapeHtml(text)}
            </div>`;
        chatMessages.appendChild(msgDiv);
        msgDiv.scrollIntoView({ behavior: 'smooth' });
    }

    function appendAiMessage(text) {
        if (!chatMessages) return;
        const msgDiv = document.createElement("div");
        msgDiv.style.display = "flex";
        msgDiv.style.gap = "15px";
        msgDiv.innerHTML = `
            <div class="avatar"><i class="bi bi-robot"></i></div>
            <div style="background:#f8fafc; padding:15px; border-radius:12px; border:1px solid #e2e8f0; max-width:650px;">
                <strong style="color:#2563eb;">EduBuddy</strong>
                <p style="white-space: pre-wrap; margin-top:5px; margin-bottom:0;">${escapeHtml(text)}</p>
            </div>`;
        chatMessages.appendChild(msgDiv);
        msgDiv.scrollIntoView({ behavior: 'smooth' });
    }

    function appendLoadingMessage() {
        const msgDiv = document.createElement("div");
        msgDiv.style.display = "flex";
        msgDiv.style.gap = "15px";
        msgDiv.innerHTML = `
            <div class="avatar"><i class="bi bi-robot"></i></div>
            <div style="background:#f8fafc; padding:12px 18px; border-radius:12px; border:1px solid #e2e8f0;">
                <strong style="color:#2563eb;">EduBuddy</strong>
                <p style="margin:5px 0 0 0; color:#64748b;"><em>EduBuddy is thinking...</em></p>
            </div>`;
        chatMessages.appendChild(msgDiv);
        msgDiv.scrollIntoView({ behavior: 'smooth' });
        return msgDiv;
    }

    // Quick Action button listeners
    document.querySelectorAll(".quick-action").forEach(button => {
        button.addEventListener("click", function () {
            const prompt = this.getAttribute("data-prompt");
            if (prompt) {
                sendChatMessage(prompt);
            }
        });
    });

    const viewStudyPlanBtn = document.getElementById("btn-view-study-plan");
    if (viewStudyPlanBtn) {
        viewStudyPlanBtn.addEventListener("click", function () {
            window.location.href = "/StudyPlan";
        });
    }

    // Dynamic Assessment Loading
    loadPendingAssessments(studentId);
});

async function loadPendingAssessments(studentId) {
    const listContainer = document.getElementById("pending-assessments-list");
    if (!listContainer) return;

    try {
        const res = await fetch(`/api/assessments/${studentId}`);
        if (res.ok) {
            const data = await res.json();
            if (data && data.length > 0) {
                listContainer.innerHTML = data.map(a => `
                    <div class="list-item">
                        <div>
                            <strong>${escapeHtml(a.title)}</strong>
                            <p>${escapeHtml(a.courseCode)} · Due: ${new Date(a.dueDate).toLocaleDateString()}</p>
                        </div>
                        <span class="badge ${a.isCompleted ? 'badge-success' : 'badge-danger'}">
                            ${a.isCompleted ? 'Completed' : 'Pending'}
                        </span>
                        ${!a.isCompleted ? `<button onclick="markAssessmentDone(${a.id})" class="btn-primary" style="margin-left:10px; padding:4px 10px; font-size:12px;">Complete</button>` : ''}
                    </div>
                `).join('');
            }
        }
    } catch (e) {
        console.warn("Could not load pending assessments:", e);
    }
}

window.markAssessmentDone = async function (id) {
    try {
        const res = await fetch(`/api/assessments/complete/${id}`, { method: 'POST' });
        if (res.ok) {
            showToast("Assessment marked as complete!", "success");
            loadPendingAssessments(1);
        } else {
            showToast("Failed to complete assessment.", "error");
        }
    } catch (e) {
        showToast("Error marking assessment complete.", "error");
    }
};

function escapeHtml(str) {
    return (str || '').replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}
