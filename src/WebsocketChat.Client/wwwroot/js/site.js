const SENT = "sent", RECEIVED = "received";

document.getElementById('chat-form').addEventListener('submit', function (event) {
    event.preventDefault();  // Prevent the form from submitting in the traditional way.
    sendMessage();
});


function createMessageElement(messageType, text, time) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message ' + messageType;
    messageDiv.textContent = text;

    const metadataSpan = document.createElement('span');
    metadataSpan.className = 'metadata';

    const timeSpan = document.createElement('span');
    timeSpan.className = 'time';
    timeSpan.textContent = time;

    metadataSpan.appendChild(timeSpan);
    messageDiv.appendChild(metadataSpan);

    return messageDiv;
}

function getCurrentTime() {
    const now = new Date();
    let hours = now.getHours();
    let minutes = now.getMinutes();

    // нули в 08:07
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;

    return `${hours}:${minutes}`;
}

function scrollToLastMessage() {
    const messagesContainer = document.querySelector('.conversation-container');
    const lastMessage = messagesContainer.lastElementChild;
    lastMessage.scrollIntoView({ behavior: 'smooth', block: 'end' });
}