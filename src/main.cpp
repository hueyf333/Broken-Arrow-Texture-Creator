#include <QAction>
#include <QApplication>
#include <QLabel>
#include <QMainWindow>
#include <QMenu>
#include <QMenuBar>
#include <QVBoxLayout>
#include <QWidget>

int main(int argc, char *argv[]) {
    QApplication app(argc, argv);

    QMainWindow window;
    window.setWindowTitle("Broken Arrow Texture Creator");

    QMenuBar *menuBar = window.menuBar();
    QMenu *fileMenu = menuBar->addMenu("&File");
    QAction *exitAction = fileMenu->addAction("E&xit");
    QObject::connect(exitAction, &QAction::triggered, &window, &QMainWindow::close);

    QWidget *viewport = new QWidget(&window);
    viewport->setObjectName("Viewport");
    viewport->setMinimumSize(640, 360);

    auto *viewportLayout = new QVBoxLayout(viewport);
    viewportLayout->setContentsMargins(0, 0, 0, 0);
    auto *viewportLabel = new QLabel("Viewport placeholder", viewport);
    viewportLabel->setAlignment(Qt::AlignCenter);
    viewportLayout->addWidget(viewportLabel);

    window.setCentralWidget(viewport);
    window.resize(1024, 768);
    window.show();

    return app.exec();
}
