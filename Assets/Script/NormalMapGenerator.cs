using UnityEngine;

public class NormalMapGenerator : MonoBehaviour
{
    public Texture2D sourceTexture; // 원본 2D 스프라이트 텍스처
    public float normalStrength = 1f; // 노멀 벡터의 강도 조절 값

    public Texture2D normalMap; // 생성된 노멀 맵 텍스처

    private void Start()
    {
        GenerateNormalMap();
    }

    private void GenerateNormalMap()
    {
        // 노멀 맵 생성을 위한 새로운 텍스처 생성
        normalMap = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

        // 픽셀 데이터 가져오기
        Color[] pixels = sourceTexture.GetPixels();

        Debug.Log(pixels.Length);

        // 픽셀 단위로 노멀 벡터 계산 및 노멀 맵 생성
        for (int y = 0; y < sourceTexture.height; y++)
        {
            for (int x = 0; x < sourceTexture.width; x++)
            {
                // 픽셀 인덱스 계산
                int index = y * sourceTexture.width + x;

                // 현재 픽셀의 주변 픽셀 위치 계산
                Vector2Int[] surroundingPixelPositions = GetSurroundingPixelPositions(x, y, sourceTexture.width, sourceTexture.height);

                // 노멀 벡터 계산
                Vector3 normal = CalculateNormal(pixels, surroundingPixelPositions);

                // 노멀 벡터를 정규화하고 강도 조절
                normal.Normalize();
                normal *= normalStrength;

                // 노멀 맵에 노멀 벡터 값 설정
                normalMap.SetPixel(x, y, new Color(normal.x, normal.y, normal.z, 1f));
            }
        }

        // 변경된 픽셀 적용
        normalMap.Apply();

        // 노멀 맵을 원하는 곳에 적용 (예: Sprite Renderer의 Normal Map 속성)
        // renderer.normalMap = normalMap;
    }

    private Vector2Int[] GetSurroundingPixelPositions(int x, int y, int width, int height)
    {
        // 8방향 (상, 하, 좌, 우, 대각선)의 주변 픽셀 위치 계산
        Vector2Int[] positions = new Vector2Int[8]
        {
            new Vector2Int(x, y + 1), // 위쪽
            new Vector2Int(x, y - 1), // 아래쪽
            new Vector2Int(x - 1, y), // 왼쪽
            new Vector2Int(x + 1, y), // 오른쪽
            new Vector2Int(x - 1, y + 1), // 왼쪽 위
            new Vector2Int(x + 1, y + 1), // 오른쪽 위
            new Vector2Int(x - 1, y - 1), // 왼쪽 아래
            new Vector2Int(x + 1, y - 1) // 오른쪽 아래
        };

        // 이미지 경계 검사하여 올바른 위치 반환
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].x = Mathf.Clamp(positions[i].x, 0, width - 1);
            positions[i].y = Mathf.Clamp(positions[i].y, 0, height - 1);
        }

        return positions;
    }

    private Vector3 CalculateNormal(Color[] pixels, Vector2Int[] surroundingPixelPositions)
    {
        Vector3 normal = Vector3.zero;

        // 주변 픽셀과의 차이 계산 및 노멀 벡터 누적
        for (int i = 0; i < surroundingPixelPositions.Length; i++)
        {
            // 현재 픽셀과 주변 픽셀 간의 차이 계산
            float difference = pixels[surroundingPixelPositions[i].y * sourceTexture.width + surroundingPixelPositions[i].x].grayscale -
                               pixels[sourceTexture.height * sourceTexture.width].grayscale;

            // 차이를 방향 벡터에 누적
            normal += new Vector3(surroundingPixelPositions[i].x - sourceTexture.width / 2,
                                  surroundingPixelPositions[i].y - sourceTexture.height / 2,
                                  difference);
        }

        // 누적된 방향 벡터 반환
        return normal;
    }
}