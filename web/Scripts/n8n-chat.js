// Nạp CSS thủ công (tránh Razor hiểu nhầm)
const link = document.createElement("link");
link.rel = "stylesheet";
link.href = "https://cdn.jsdelivr.net/npm/@n8n/chat/dist/style.css";
document.head.appendChild(link);

// Import và khởi tạo n8n chat
import { createChat } from "https://cdn.jsdelivr.net/npm/@n8n/chat/dist/chat.bundle.es.js";

createChat({
    container: "#n8n-chat-container",
    webhookUrl: "http://localhost:5678/webhook/5f1c0c82-0ff9-40c7-9e2e-b1a96ffe24cd/chat",
    placeholder: "Gửi tin nhắn…",
    title: "Hỗ trợ Chatbot",
    subtitle: "Mình sẵn sàng trả lời bạn!",
    theme: {
        primaryColor: "#007bff",
        headerBackground: "#0056b3",
    },
    allowAnonymous: true,
});
