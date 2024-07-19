uniform sampler2D texture;
uniform float time;
uniform float intensity;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main() {
    vec2 texCoord = gl_TexCoord[0].xy;
    float randomVal = rand(texCoord + time);
    vec2 shake = vec2(rand(texCoord + time * 2.0) - 0.5, rand(texCoord + time * 3.0) - 0.5) * intensity;
    
    vec4 color = texture2D(texture, texCoord + shake);
    gl_FragColor = color;
}
